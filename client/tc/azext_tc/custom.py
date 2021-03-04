# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------
# pylint: disable=unused-argument, protected-access, too-many-lines

from knack.util import CLIError
from knack.log import get_logger

logger = get_logger(__name__)


def _ensure_base_url(client, base_url):
    client._client._base_url = base_url

# TeamCloud


# def teamcloud_test(cmd, base_url):
#     pass


def teamcloud_update(cmd, version=None, prerelease=False):  # pylint: disable=too-many-statements, too-many-locals
    import os
    import tempfile
    import shutil
    from azure.cli.core import CommandIndex
    from azure.cli.core.extension import get_extension
    from azure.cli.core.extension.operations import _add_whl_ext, _augment_telemetry_with_ext_info
    from ._deploy_utils import get_github_release

    release = get_github_release(cmd.cli_ctx, 'TeamCloud', version=version, prerelease=prerelease)
    asset = next((a for a in release['assets']
                  if 'py3-none-any.whl' in a['browser_download_url']), None)

    download_url = asset['browser_download_url'] if asset else None

    if not download_url:
        raise CLIError(
            'Could not find extension .whl asset on release {}. '
            'Specify a specific prerelease version with --version '
            'or use latest prerelease with --pre'.format(release['tag_name']))

    extension_name = 'tc'
    ext = get_extension(extension_name)
    cur_version = ext.get_version()
    # Copy current version of extension to tmp directory in case we need to restore it after a failed install.
    backup_dir = os.path.join(tempfile.mkdtemp(), extension_name)
    extension_path = ext.path
    logger.debug('Backing up the current extension: %s to %s', extension_path, backup_dir)
    shutil.copytree(extension_path, backup_dir)
    # Remove current version of the extension
    shutil.rmtree(extension_path)
    # Install newer version
    try:
        _add_whl_ext(cli_ctx=cmd.cli_ctx, source=download_url)
        logger.debug('Deleting backup of old extension at %s', backup_dir)
        shutil.rmtree(backup_dir)
        # This gets the metadata for the extension *after* the update
        _augment_telemetry_with_ext_info(extension_name)
    except Exception as err:
        logger.error('An error occurred whilst updating.')
        logger.error(err)
        logger.debug('Copying %s to %s', backup_dir, extension_path)
        shutil.copytree(backup_dir, extension_path)
        raise CLIError('Failed to update. Rolled {} back to {}.'.format(
            extension_name, cur_version))
    CommandIndex().invalidate()


def teamcloud_deploy(cmd, client, name, location=None, resource_group_name='TeamCloud',  # pylint: disable=too-many-statements, too-many-locals
                     principal_name=None, principal_password=None, tags=None, version=None,
                     skip_app_deployment=False, skip_name_validation=False, prerelease=False,
                     index_url=None):
    from azure.cli.core._profile import Profile
    from ._deploy_utils import (
        get_index_teamcloud, deploy_arm_template_at_resource_group, get_resource_group_by_name,
        create_resource_group_name, create_resource_manager_sp, set_appconfig_keys, zip_deploy_app,
        get_arm_output)

    cli_ctx = cmd.cli_ctx

    hook = cli_ctx.get_progress_controller()
    hook.begin()

    hook.add(message='Fetching index.json from GitHub')
    version, deploy_url, api_zip_url, orchestrator_zip_url = get_index_teamcloud(
        cli_ctx, version, prerelease, index_url)

    hook.add(message='Getting resource group {}'.format(resource_group_name))
    rg, _ = get_resource_group_by_name(cli_ctx, resource_group_name)
    if rg is None:
        if location is None:
            raise CLIError(
                "--location/-l is required if resource group '{}' does not exist".format(resource_group_name))
        hook.add(message="Resource group '{}' not found".format(resource_group_name))
        hook.add(message="Creating resource group '{}'".format(resource_group_name))
        rg, _ = create_resource_group_name(cli_ctx, resource_group_name, location)

    profile = Profile(cli_ctx=cli_ctx)

    if principal_name is None and principal_password is None:
        hook.add(message='Creating AAD app registration')
        resource_manager_sp = create_resource_manager_sp(cmd, name)
    else:
        _, _, tenant_id = profile.get_login_credentials(
            resource=cli_ctx.cloud.endpoints.active_directory_graph_resource_id)
        resource_manager_sp = {
            'appId': principal_name,
            'password': principal_password,
            'tenant': tenant_id
        }

    parameters = []
    parameters.append('webAppName={}'.format(name))
    parameters.append('resourceManagerIdentityClientId={}'.format(resource_manager_sp['appId']))
    parameters.append('resourceManagerIdentityClientSecret={}'.format(
        resource_manager_sp['password']))

    hook.add(message='Deploying ARM template')
    outputs = deploy_arm_template_at_resource_group(
        cmd, resource_group_name, template_uri=deploy_url, parameters=[parameters])

    api_url = get_arm_output(outputs, 'apiUrl')
    orchestrator_url = get_arm_output(outputs, 'orchestratorUrl')
    api_app_name = get_arm_output(outputs, 'apiAppName')
    orchestrator_app_name = get_arm_output(outputs, 'orchestratorAppName')
    config_service_conn_string = get_arm_output(outputs, 'configServiceConnectionString')
    config_service_imports = get_arm_output(outputs, 'configServiceImport')

    config_kvs = []
    for k, v in config_service_imports.items():
        config_kvs.append({'key': k, 'value': v})

    hook.add(message='Adding ARM template outputs to App Configuration service')
    set_appconfig_keys(cmd, config_service_conn_string, config_kvs)

    if skip_app_deployment:
        logger.warning(
            'IMPORTANT: --skip-app-deployment prevented source code for the TeamCloud instance deployment. '
            'To deploy the applications use `az tc upgrade`.')
    else:
        hook.add(message='Deploying Orchestrator source code')
        zip_deploy_app(cli_ctx, resource_group_name, orchestrator_app_name, orchestrator_zip_url)

        hook.add(message='Deploying API source code')
        zip_deploy_app(cli_ctx, resource_group_name, api_app_name, api_zip_url)

        version_string = version or 'the latest version'
        hook.add(message='Successfully created TeamCloud instance ({})'.format(version_string))

    hook.end(message=' ')
    logger.warning(' ')
    logger.warning('TeamCloud instance successfully created at: %s', api_url)
    logger.warning('Use `az configure --defaults tc-base-url=%s` to configure '
                   'this as your default TeamCloud instance', api_url)

    result = {
        'deployed': not skip_app_deployment,
        'version': version or 'latest',
        'name': name,
        'base_url': api_url,
        'location': rg.location,
        'api': {
            'name': api_app_name,
            'url': api_url
        },
        'orchestrator': {
            'name': orchestrator_app_name,
            'url': orchestrator_url
        },
        'service_principal': {
            'appId': resource_manager_sp['appId'],
            # 'password': resource_manager_sp['password'],
            'tenant': resource_manager_sp['tenant']
        }
    }

    return result


def teamcloud_app_deploy(cmd, client, base_url, client_id, app_type='Web', version=None,  # pylint: disable=too-many-locals
                         scope=None, prerelease=False, index_url=None):
    from ._deploy_utils import (
        get_index_webapp, get_resource_group_by_name, create_resource_group_name,
        deploy_arm_template_at_resource_group, zip_deploy_app, get_app_info)
    from msrestazure.tools import parse_resource_id
    from azure.cli.core.commands.client_factory import get_subscription_id

    cli_ctx = cmd.cli_ctx

    if app_type.lower() != 'web':
        raise CLIError("--type/-t currently only supports 'Web'")

    hook = cli_ctx.get_progress_controller()
    hook.begin()

    hook.add(message='Fetching index.json from GitHub')
    version, deploy_url, zip_url = get_index_webapp(cli_ctx, version, prerelease, index_url)

    app = get_app_info(cmd, base_url)
    rid = parse_resource_id(app.id)

    subscription_id = get_subscription_id(cmd.cli_ctx)

    if subscription_id != rid['subscription']:
        raise CLIError('TeamCloud instance is deployed in a different subscription than the current')

    name = rid['resource_name'] + '-' + app_type.lower()
    url = 'https://{}.azurewebsites.net'.format(name)

    resource_group_name = rid['resource_group'] + '-' + app_type

    rg, _ = get_resource_group_by_name(cli_ctx, resource_group_name)
    if rg is None:
        hook.add(message="Creating resource group '{}'".format(resource_group_name))
        rg, _ = create_resource_group_name(cli_ctx, resource_group_name, app.location)

    parameters = []
    parameters.append('webAppName={}'.format(name))
    parameters.append('reactAppMsalClientId={}'.format(client_id))
    parameters.append('reactAppTcApiUrl={}'.format(base_url))

    if scope:
        parameters.append('reactAppMsalScope={}'.format(scope))

    hook.add(message='Deploying ARM template')
    _ = deploy_arm_template_at_resource_group(
        cmd, resource_group_name, template_uri=deploy_url, parameters=[parameters])

    hook.add(message='Deploying {} app source code'.format(app_type))
    zip_deploy_app(cli_ctx, resource_group_name, name, zip_url)

    result = {
        'version': version or 'latest',
        'name': name,
        'url': '{}'.format(url),
    }

    return result


# Orgs

def org_create(cmd, client, base_url, name, location=None):
    from .vendored_sdks.teamcloud.models import OrganizationDefinition
    from azure.cli.core.commands.client_factory import get_subscription_id

    subscription_id = get_subscription_id(cmd.cli_ctx)

    payload = OrganizationDefinition(display_name=name, subscription_id=subscription_id, location=location)

    return _create(cmd, client, base_url, client.create_organization, payload)


def org_delete(cmd, client, base_url, org):
    return _delete(cmd, client, base_url, client.delete_organization, org)


def org_list(cmd, client, base_url):
    return _list(cmd, client, base_url, client.get_organizations)


def org_get(cmd, client, base_url, org):
    return _get(cmd, client, base_url, client.get_organization, org)


# Deployment Scopes

def deployment_scope_create(cmd, client, base_url, org, scope, subscriptions, default=False):
    from .vendored_sdks.teamcloud.models import DeploymentScopeDefinition

    payload = DeploymentScopeDefinition(display_name=scope, subscription_ids=subscriptions, is_default=default)

    return _create(cmd, client, base_url, client.create_deployment_scope, payload, org=org)


def deployment_scope_delete(cmd, client, base_url, org, scope):
    return _delete(cmd, client, base_url, client.delete_deployment_scope, scope, org=org)


def deployment_scope_list(cmd, client, base_url, org):
    return _list(cmd, client, base_url, client.get_deployment_scopes, org=org)


def deployment_scope_get(cmd, client, base_url, org, scope):
    return _get(cmd, client, base_url, client.get_deployment_scope, scope, org=org)


# Project Templates

def project_template_create(cmd, client, base_url, org, template, repo_url, repo_version=None, repo_token=None):
    from .vendored_sdks.teamcloud.models import ProjectTemplateDefinition, RepositoryDefinition
    repository = RepositoryDefinition(url=repo_url, version=repo_version, token=repo_token)
    payload = ProjectTemplateDefinition(display_name=template, repository=repository)
    return _create(cmd, client, base_url, client.create_project_template, payload, org=org)


def project_template_delete(cmd, client, base_url, org, template):
    return _delete(cmd, client, base_url, client.delete_project_template, template, org=org)


def project_template_list(cmd, client, base_url, org):
    return _list(cmd, client, base_url, client.get_project_templates, org=org)


def project_template_get(cmd, client, base_url, org, template):
    return _get(cmd, client, base_url, client.get_project_template, template, org=org)


# Common

def _create(cmd, client, base_url, func, payload, org=None, project=None):
    _ensure_base_url(client, base_url)
    return func(org, project, payload) if org and project else func(org, payload) if org else func(payload)


def _delete(cmd, client, base_url, func, item, org=None, project=None):
    _ensure_base_url(client, base_url)
    return func(org, project, item) if org and project else func(org, item) if org else func(item)


def _list(cmd, client, base_url, func, org=None, project=None):
    _ensure_base_url(client, base_url)
    return func(org, project) if org and project else func(org) if org else func()


def _get(cmd, client, base_url, func, item, org=None, project=None):
    _ensure_base_url(client, base_url)
    return func(org, project, item) if org and project else func(org, item) if org else func(item)

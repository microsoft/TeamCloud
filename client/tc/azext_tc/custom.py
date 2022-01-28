# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------
# pylint: disable=unused-argument, protected-access, too-many-lines
# pylint: disable=too-many-locals, too-many-statements

from knack.util import CLIError
from knack.log import get_logger

logger = get_logger(__name__)


def _ensure_base_url(client, base_url):
    client._client._base_url = base_url

# TeamCloud


def teamcloud_update(cmd, version=None, prerelease=False):
    from azure.cli.core.extension.operations import update_extension
    from ._deploy_utils import get_github_release

    release = get_github_release('TeamCloud', version=version, prerelease=prerelease)

    index = next((a for a in release['assets']
                  if 'index.json' in a['browser_download_url']), None)

    index_url = index['browser_download_url'] if index else None

    if not index_url:
        raise CLIError(
            f"Could not find index.json asset on release {release['tag_name']}. "
            'Specify a specific prerelease version with --version '
            'or use latest prerelease with --pre')

    update_extension(cmd, extension_name='tc', index_url=index_url)


def teamcloud_deploy(cmd, name, client_id, location=None, resource_group_name='TeamCloud',
                     principal_name=None, principal_password=None, tags=None, version=None,
                     skip_app_deployment=False, skip_name_validation=False, prerelease=False,
                     index_file=None, index_url=None, scope=None):
    from azure.cli.core._profile import Profile
    from ._deploy_utils import (
        deploy_arm_template_at_resource_group, get_resource_group_by_name, get_arm_output,
        create_resource_group_name, create_resource_manager_sp, get_teamcloud_index)

    cli_ctx = cmd.cli_ctx

    hook = cli_ctx.get_progress_controller()
    hook.begin()

    hook.add(message='Fetching index.json from GitHub')
    version, deploy_url = get_teamcloud_index(version, prerelease, index_file, index_url)

    hook.add(message=f'Getting resource group {resource_group_name}')
    rg, _ = get_resource_group_by_name(cli_ctx, resource_group_name)
    if rg is None:
        if location is None:
            raise CLIError(
                f"--location/-l is required if resource group '{resource_group_name}' does not exist")
        hook.add(message=f"Resource group '{resource_group_name}' not found")
        hook.add(message=f"Creating resource group '{resource_group_name}'")
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
    parameters.append(f'doSleepHack={skip_name_validation is False}')
    parameters.append(f'webAppName={name}')
    parameters.append(f"resourceManagerIdentityClientId={resource_manager_sp['appId']}")
    parameters.append(f"resourceManagerIdentityClientSecret={resource_manager_sp['password']}")
    parameters.append(f'reactAppMsalClientId={client_id}')

    if version:
        parameters.append(f'reactAppVersion={version}')

    if index_file:
        parameters.append(f'reactAppVersion=local')

    if scope:
        parameters.append(f'reactAppMsalScope={scope}')

    remote_deploy = deploy_url.lower().startswith('https://') or deploy_url.lower().startswith('http://')

    hook.add(message='Deploying ARM template')
    outputs = deploy_arm_template_at_resource_group(
        cmd, resource_group_name, template_file=(deploy_url if not remote_deploy else None),
        template_uri=(deploy_url if remote_deploy else None), parameters=[parameters])

    api_app_name = get_arm_output(outputs, 'apiAppName')
    web_app_name = get_arm_output(outputs, 'webAppName')
    orchestrator_app_name = get_arm_output(outputs, 'orchestratorAppName')

    api_url = get_arm_output(outputs, 'apiUrl')
    web_url = get_arm_output(outputs, 'webUrl')
    orchestrator_url = get_arm_output(outputs, 'orchestratorUrl')

    if skip_app_deployment:
        logger.warning(
            'IMPORTANT: --skip-app-deployment prevented source code for the TeamCloud instance deployment. '
            'To deploy the applications use `az tc upgrade`.')
    else:
        version_string = version or 'the latest version'
        hook.add(message=f'Successfully created TeamCloud instance ({version_string})')

    hook.end(message=' ')
    logger.warning(' ')
    logger.warning('TeamCloud instance successfully created at: %s', api_url)
    logger.warning('Use `az configure -d tc-url=%s` to set '
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
        'web': {
            'name': web_app_name,
            'url': web_url
        },
        'service_principal': {
            'appId': resource_manager_sp['appId'],
            'tenant': resource_manager_sp['tenant']
        }
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

def deployment_scope_create(cmd, client, base_url, org, scope, scope_type='AzureResourceManager', parameters=None):
    _ensure_base_url(client, base_url)

    import json
    from .vendored_sdks.teamcloud.models import DeploymentScopeDefinition
    from ._input_utils import (_process_parameters, _get_missing_parameters, _prompt_for_parameters,
                               _get_best_match_one_of)

    if parameters is None:
        parameters = []

    adapters = client.get_adapters()

    adapter = next((a for a in adapters.data if a.type == scope_type), None)
    if adapter is None:
        raise CLIError(f"Adapter not found of type '{scope_type}'")

    input_data_schema = json.loads(adapter.input_data_schema)
    input_ui_schema = json.loads(adapter.input_data_form)

    input_data_schema_one_of = input_data_schema.get('oneOf', None)
    if input_data_schema_one_of is not None:
        input_data_schema = _get_best_match_one_of(input_data_schema, parameters)

    parameters = _process_parameters(input_data_schema, parameters) or {}
    parameters = _get_missing_parameters(parameters, input_data_schema, _prompt_for_parameters, input_ui_schema)

    parameters = json.loads(json.dumps(parameters))

    payload = DeploymentScopeDefinition(display_name=scope, type=scope_type, input_date=parameters)

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

def _create(cmd, client, base_url, func, payload, org=None, project=None, component=None):
    _ensure_base_url(client, base_url)
    return func(org, project, component, payload) if org and project and component \
        else func(org, project, payload) if org and project \
        else func(org, payload) if org else func(payload)


def _delete(cmd, client, base_url, func, item, org=None, project=None, component=None):
    _ensure_base_url(client, base_url)
    return func(item, org, project, component) if org and project and component \
        else func(item, org, project) if org and project \
        else func(item, org) if org else func(item)


def _list(cmd, client, base_url, func, org=None, project=None, component=None):
    _ensure_base_url(client, base_url)
    return func(org, project, component) if org and project and component \
        else func(org, project) if org and project \
        else func(org) if org else func()


def _get(cmd, client, base_url, func, item, org=None, project=None, component=None):
    _ensure_base_url(client, base_url)
    return func(item, org, project, component) if org and project and component \
        else func(item, org, project) if org and project \
        else func(item, org) if org else func(item)

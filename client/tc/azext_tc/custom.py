# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------
# pylint: disable=unused-argument, protected-access, too-many-lines

from time import sleep
from urllib.parse import urlparse
from knack.util import CLIError
from knack.log import get_logger
from azure.cli.core.util import sdk_no_wait

logger = get_logger(__name__)

STATUS_POLLING_SLEEP_INTERVAL = 2


# TeamCloud

def teamcloud_info(cmd, client, base_url):
    client._client.config.base_url = base_url
    return client.get_team_cloud_instance()


def teamcloud_deploy(cmd, client, name, location=None, resource_group_name='TeamCloud',  # pylint: disable=too-many-statements, too-many-locals
                     principal_name=None, principal_password=None, tags=None, version=None,
                     skip_app_deployment=False, skip_name_validation=False, skip_admin_user=False,
                     prerelease=False, index_url=None):
    from re import sub
    from azure.cli.core._profile import Profile
    from .vendored_sdks.teamcloud.models import UserDefinition, TeamCloudInstance, AzureResourceGroup
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
    rg, sub_id = get_resource_group_by_name(cli_ctx, resource_group_name)
    if rg is None:
        if location is None:
            raise CLIError(
                "--location/-l is required if resource group '{}' does not exist".format(resource_group_name))
        hook.add(message="Resource group '{}' not found".format(resource_group_name))
        hook.add(message="Creating resource group '{}'".format(resource_group_name))
        rg, sub_id = create_resource_group_name(cli_ctx, resource_group_name, location)

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
    set_appconfig_keys(cli_ctx, config_service_conn_string, config_kvs)

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

    client._client.config.base_url = api_url

    if skip_admin_user:
        logger.warning(
            'IMPORTANT: --skip-admin-user prevented adding you as an Admin user to the TeamCloud instance deployment.')
    else:
        me = profile.get_current_account_user()
        me = sub('http[s]?://', '', me)
        hook.add(message="Adding '{}' as an admin user".format(me))

        user_definition = UserDefinition(identifier=me, role='Admin', properties=None)
        _ = client.create_team_cloud_admin_user(user_definition)

    hook.add(message='Adding TeamCloud instance information')
    resource_group = AzureResourceGroup(
        id=rg.id, name=rg.name, region=rg.location, subscription_id=sub_id)
    teamcloud_instance = TeamCloudInstance(
        version=version, resource_group=resource_group, tags=tags)
    _ = client.create_team_cloud_instance(teamcloud_instance)

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


def teamcloud_upgrade(cmd, client, base_url, version=None, prerelease=False, index_url=None):  # pylint: disable=too-many-statements, too-many-locals
    from ._deploy_utils import (
        get_index_teamcloud, deploy_arm_template_at_resource_group, get_resource_group_by_name,
        set_appconfig_keys, zip_deploy_app, get_app_name, get_arm_output)

    client._client.config.base_url = base_url
    cli_ctx = cmd.cli_ctx

    hook = cli_ctx.get_progress_controller()
    hook.begin()

    hook.add(message='Fetching index.json from GitHub')
    version, deploy_url, api_zip_url, orchestrator_zip_url = get_index_teamcloud(
        cli_ctx, version, prerelease, index_url)

    hook.add(message='Getting TeamCloud instance information')
    tc_instance_result = client.get_team_cloud_instance()

    if version and tc_instance_result.data.version and version == tc_instance_result.data.version:
        raise CLIError("TeamCloud instance is already using version {}".format(version))

    name = get_app_name(base_url)

    resource_group_name = tc_instance_result.data.resource_group.name
    if not resource_group_name:
        raise CLIError('TODO TeamCloud instance was not deployed by the cli')

    hook.add(message='Getting resource group {}'.format(resource_group_name))
    rg, _ = get_resource_group_by_name(cli_ctx, resource_group_name)
    if rg is None:
        raise CLIError(
            "Resource Group '{}' must exist in current subscription.".format(resource_group_name))

    parameters = []
    parameters.append('webAppName={}'.format(name))
    parameters.append('resourceManagerIdentityClientId=')
    parameters.append('resourceManagerIdentityClientSecret=')

    hook.add(message='Deploying ARM template')
    outputs = deploy_arm_template_at_resource_group(
        cmd, resource_group_name, template_uri=deploy_url, parameters=[parameters])

    api_app_name = get_arm_output(outputs, 'apiAppName')
    orchestrator_app_name = get_arm_output(outputs, 'orchestratorAppName')
    config_service_conn_string = get_arm_output(outputs, 'configServiceConnectionString')
    config_service_imports = get_arm_output(outputs, 'configServiceImport')

    config_kvs = []
    for k, v in config_service_imports.items():
        if v:
            config_kvs.append({'key': k, 'value': v})

    hook.add(message='Adding ARM template outputs to App Configuration service')
    set_appconfig_keys(cli_ctx, config_service_conn_string, config_kvs)

    hook.add(message='Deploying Orchestrator source code')
    zip_deploy_app(cli_ctx, resource_group_name, orchestrator_app_name, orchestrator_zip_url)

    hook.add(message='Deploying API source code')
    zip_deploy_app(cli_ctx, resource_group_name, api_app_name, api_zip_url)

    hook.add(message='Updating TeamCloud instance information')
    payload = tc_instance_result.data
    payload.version = version

    _ = client.update_team_cloud_instance(payload)

    hook.end(message=' ')
    logger.warning(' ')
    logger.warning('Successfully upgraded TeamCloud instance to %s', version or '')

    result = {
        'version': version or 'latest',
        'name': name,
        'base_url': '{}'.format(base_url),
        'api': {
            'name': api_app_name
        },
        'orchestrator': {
            'name': orchestrator_app_name
        }
    }

    return result


def status_get(cmd, client, base_url, tracking_id, project=None):
    client._client.config.base_url = base_url
    return client.get_project_status(project, tracking_id) if project else client.get_status(tracking_id)


# TeamCloud App

def teamcloud_app_deploy(cmd, client, base_url, client_id, app_type='Web', version=None,  # pylint: disable=too-many-locals
                         prerelease=False, index_url=None, location=None, resource_group_name=None):
    from ._deploy_utils import (
        get_index_webapp, get_app_name, get_resource_group_by_name, create_resource_group_name,
        deploy_arm_template_at_resource_group, zip_deploy_app)
    from .vendored_sdks.teamcloud.models import TeamCloudApplication, AzureResourceGroup

    client._client.config.base_url = base_url
    cli_ctx = cmd.cli_ctx

    if app_type.lower() != 'web':
        raise CLIError("--type/-t currently only supports 'Web'")

    hook = cli_ctx.get_progress_controller()
    hook.begin()

    hook.add(message='Fetching index.json from GitHub')
    version, deploy_url, zip_url = get_index_webapp(cli_ctx, version, prerelease, index_url)

    hook.add(message='Getting TeamCloud instance information')
    tc_instance_result = client.get_team_cloud_instance()
    tc_instance = tc_instance_result.data

    index = next((i for i, a in enumerate(tc_instance.applications)
                  if a.type.lower() == app_type.lower()), None)
    if index is not None:
        raise CLIError('A {} app already exists for this TeamCloud instance'.format(app_type.lower))

    name = get_app_name(base_url) + '-' + app_type.lower()
    url = 'https://{}.azurewebsites.net'.format(name)

    if resource_group_name is None:
        resource_group_name = tc_instance.resource_group.name
        if not resource_group_name:
            raise CLIError('TODO TeamCloud instance was not deployed by the cli')
        location = tc_instance.resource_group.region
        hook.add(message='Getting resource group {}'.format(resource_group_name))
        rg, sub_id = get_resource_group_by_name(cli_ctx, resource_group_name)
        if rg is None:
            raise CLIError(
                "TeamCloud instance resource group '{}' not found".format(resource_group_name))
    else:
        rg, sub_id = get_resource_group_by_name(cli_ctx, resource_group_name)
        if rg is None:
            if location is None:
                raise CLIError(
                    "--location/-l is required if resource group '{}' does not exist".format(resource_group_name))
            hook.add(message="Resource group '{}' not found".format(resource_group_name))
            hook.add(message="Creating resource group '{}'".format(resource_group_name))
            rg, sub_id = create_resource_group_name(cli_ctx, resource_group_name, location)

    parameters = []
    parameters.append('webAppName={}'.format(name))
    # if client_id:
    parameters.append('reactAppMsalClientId={}'.format(client_id))
    parameters.append('reactAppTcApiUrl={}'.format(base_url))

    hook.add(message='Deploying ARM template')
    _ = deploy_arm_template_at_resource_group(
        cmd, resource_group_name, template_uri=deploy_url, parameters=[parameters])

    hook.add(message='Deploying {} app source code'.format(app_type.lower()))
    zip_deploy_app(cli_ctx, resource_group_name, name, zip_url)

    resource_group = AzureResourceGroup(
        id=rg.id, name=rg.name, region=rg.location, subscription_id=sub_id)
    tc_application = TeamCloudApplication(
        url=url, version=version, type=app_type, resource_group=resource_group)

    tc_instance.applications.append(tc_application)
    _ = client.update_team_cloud_instance(tc_instance)

    result = {
        'version': version or 'latest',
        'name': name,
        'url': '{}'.format(url),
    }

    return result


def teamcloud_app_upgrade(cmd, client, base_url, client_id, app_type='Web', version=None,  # pylint: disable=too-many-locals
                          prerelease=False, index_url=None):
    from ._deploy_utils import (
        get_index_webapp, get_app_name, get_resource_group_by_name,
        deploy_arm_template_at_resource_group, zip_deploy_app)

    client._client.config.base_url = base_url
    cli_ctx = cmd.cli_ctx

    # if app_type.lower() != 'web':
    #     raise CLIError("--type/-t currently only supports 'Web'")

    hook = cli_ctx.get_progress_controller()
    hook.begin()

    hook.add(message='Fetching index.json from GitHub')
    version, deploy_url, zip_url = get_index_webapp(cli_ctx, version, prerelease, index_url)

    hook.add(message='Getting TeamCloud instance information')
    tc_instance_result = client.get_team_cloud_instance()
    tc_instance = tc_instance_result.data

    index = next((i for i, a in enumerate(tc_instance.applications)
                  if a.type.lower() == app_type.lower()), None)
    if index is None:
        raise CLIError('No web app found for TeamCloud instance: {}'.format(base_url))
    web_app = tc_instance.applications[index]

    name = get_app_name(web_app.url)
    resource_group_name = web_app.resource_group.name

    if version and web_app.version and version == web_app.version:
        raise CLIError("TeamCloud {} app '{}' is already using version {}".format(
            app_type.lower(), name, version))

    hook.add(message='Getting resource group {}'.format(resource_group_name))
    rg, _ = get_resource_group_by_name(cli_ctx, resource_group_name)
    if rg is None:
        raise CLIError(
            "Resource Group '{}' must exist in current subscription.".format(resource_group_name))

    parameters = []
    parameters.append('webAppName={}'.format(name))
    # if client_id:
    parameters.append('reactAppMsalClientId={}'.format(client_id))
    parameters.append('reactAppTcApiUrl={}'.format(base_url))

    hook.add(message='Deploying ARM template')
    _ = deploy_arm_template_at_resource_group(
        cmd, resource_group_name, template_uri=deploy_url, parameters=[parameters])

    hook.add(message='Deploying {} app source code'.format(app_type.lower()))
    zip_deploy_app(cli_ctx, resource_group_name, name, zip_url)

    tc_instance.applications[index].version = version
    _ = client.update_team_cloud_instance(tc_instance)

    result = {
        'version': version or 'latest',
        'name': name,
        'url': '{}'.format(web_app.url),
    }

    return result


# TeamCloud Users

def teamcloud_user_create(cmd, client, base_url, user, role='Creator', properties=None, no_wait=False):
    from .vendored_sdks.teamcloud.models import UserDefinition

    payload = UserDefinition(identifier=user, role=role, properties=properties)

    return _create_with_status(cmd, client, base_url, payload, client.create_team_cloud_user, no_wait=no_wait)


def teamcloud_user_delete(cmd, client, base_url, user, no_wait=False):
    return _delete_with_status(cmd, client, base_url, user, client.delete_team_cloud_user, no_wait=no_wait)


def teamcloud_user_list(cmd, client, base_url):
    client._client.config.base_url = base_url
    return client.get_team_cloud_users()


def teamcloud_user_get(cmd, client, base_url, user):
    client._client.config.base_url = base_url
    return client.get_team_cloud_user_by_name_or_id(user)


def teamcloud_user_update(cmd, client, base_url, user, instance, role=None, properties=None):
    if role:
        instance.role = role
    if properties:
        instance.properties = instance.properties or {}
        instance.properties.update(properties)
        for key in [k for k, v in instance.properties.items() if not v]:
            instance.properties.pop(key)
    return instance


def teamcloud_user_get_for_update(cmd, client, base_url, user):
    from ._transformers import transform_output
    client._client.config.base_url = base_url
    instance = client.get_team_cloud_user_by_name_or_id(user)
    return transform_output(instance)


def teamcloud_user_set_for_update(cmd, client, base_url, payload):
    from ._transformers import transform_output
    instance = _update_with_status(cmd, client, base_url, payload, client.update_team_cloud_user)
    return transform_output(instance)


# TeamCloud Tags

def teamcloud_tag_create(cmd, client, base_url, tag_key, tag_value, no_wait=False):
    payload = {tag_key, tag_value}

    return _create_with_status(cmd, client, base_url, payload, client.create_team_cloud_tag, no_wait=no_wait)


def teamcloud_tag_delete(cmd, client, base_url, tag_key, no_wait=False):
    return _delete_with_status(cmd, client, base_url, tag_key, client.delete_team_cloud_tag, no_wait=no_wait)


def teamcloud_tag_list(cmd, client, base_url):
    client._client.config.base_url = base_url
    return client.get_team_cloud_tags()


def teamcloud_tag_get(cmd, client, base_url, tag_key):
    client._client.config.base_url = base_url
    return client.get_team_cloud_tag_by_key(tag_key)


# Projects

def project_create(cmd, client, base_url, name, project_type=None, tags=None,
                   properties=None, no_wait=False):
    from .vendored_sdks.teamcloud.models import ProjectDefinition

    payload = ProjectDefinition(name=name, project_type=project_type,
                                tags=tags, properties=properties)

    return _create_with_status(cmd, client, base_url, payload, client.create_project, no_wait=no_wait)


def project_delete(cmd, client, base_url, project, no_wait=False):
    return _delete_with_status(cmd, client, base_url, project, client.delete_project, no_wait=no_wait)


def project_list(cmd, client, base_url):
    client._client.config.base_url = base_url
    return client.get_projects()


def project_get(cmd, client, base_url, project):
    client._client.config.base_url = base_url
    return client.get_project_by_name_or_id(project)


# Project Users

def project_user_create(cmd, client, base_url, project, user, role='Member', properties=None, no_wait=False):
    from .vendored_sdks.teamcloud.models import UserDefinition

    payload = UserDefinition(identifier=user, role=role, properties=properties)

    return _create_with_status(cmd, client, base_url, payload, client.create_project_user,
                               project_id=project, no_wait=no_wait)


def project_user_delete(cmd, client, base_url, project, user, no_wait=False):
    return _delete_with_status(cmd, client, base_url, user, client.delete_project_user,
                               project_id=project, no_wait=no_wait)


def project_user_list(cmd, client, base_url, project):
    client._client.config.base_url = base_url
    return client.get_project_users(project)


def project_user_get(cmd, client, base_url, project, user):
    client._client.config.base_url = base_url
    return client.get_project_user_by_name_or_id(user, project)


def project_user_update(cmd, client, base_url, project, user, instance, role=None, properties=None):
    index = next((i for i, m in enumerate(instance.project_memberships)
                  if m.project_id == project), None)
    if role:
        instance.project_memberships[index].role = role
    if properties:
        instance.project_memberships[index].properties = instance.project_memberships[
            index].properties or {}
        instance.project_memberships[index].properties.update(properties)
        for key in [k for k, v in instance.project_memberships[index].properties.items() if not v]:
            instance.project_memberships[index].properties.pop(key)
    return instance


def project_user_get_for_update(cmd, client, base_url, project, user):
    from ._transformers import transform_output
    client._client.config.base_url = base_url
    instance = client.get_project_user_by_name_or_id(user, project)
    return transform_output(instance)


def project_user_set_for_update(cmd, client, base_url, project, payload):
    from ._transformers import transform_output
    instance = _update_with_status(cmd, client, base_url, payload, client.update_project_user,
                                   project_id=project)
    return transform_output(instance)


# Project Tags

def project_tag_create(cmd, client, base_url, project, tag_key, tag_value, no_wait=False):
    payload = {tag_key, tag_value}

    return _create_with_status(cmd, client, base_url, payload, client.create_project_tag,
                               project_id=project, no_wait=no_wait)


def project_tag_delete(cmd, client, base_url, project, tag_key, no_wait=False):
    return _delete_with_status(cmd, client, base_url, tag_key, client.delete_project_tag,
                               project_id=project, no_wait=no_wait)


def project_tag_list(cmd, client, base_url, project):
    client._client.config.base_url = base_url
    return client.get_project_tags(project)


def project_tag_get(cmd, client, base_url, project, tag_key):
    client._client.config.base_url = base_url
    return client.get_project_tag_by_key(project, tag_key)


# Project Types

def project_type_create(cmd, client, base_url, project_type, subscriptions, provider, providers,
                        location, subscription_capacity=10, resource_group_name_prefix=None,
                        tags=None, properties=None, default=False):
    from .vendored_sdks.teamcloud.models import ProjectType
    client._client.config.base_url = base_url

    payload = ProjectType(id=project_type, is_default=default, region=location,
                          subscriptions=subscriptions, subscription_capacity=subscription_capacity,
                          resource_group_name_prefix=resource_group_name_prefix, providers=providers,
                          tags=tags, properties=properties)

    return client.create_project_type(payload)


def project_type_delete(cmd, client, base_url, project_type):
    client._client.config.base_url = base_url
    return client.delete_project_type(project_type)


def project_type_list(cmd, client, base_url):
    client._client.config.base_url = base_url
    return client.get_project_types()


def project_type_get(cmd, client, base_url, project_type):
    client._client.config.base_url = base_url
    return client.get_project_type_by_id(project_type)


# Providers

def provider_create(cmd, client, base_url, provider, url, auth_code, events=None, properties=None, no_wait=False):
    from .vendored_sdks.teamcloud.models import Provider

    payload = Provider(id=provider, url=url, auth_code=auth_code,
                       events=events, properties=properties)

    return _create_with_status(cmd, client, base_url, payload, client.create_provider, no_wait=no_wait)


def provider_delete(cmd, client, base_url, provider, no_wait=False):
    return _delete_with_status(cmd, client, base_url, provider, client.delete_provider, no_wait=no_wait)


def provider_list(cmd, client, base_url):
    client._client.config.base_url = base_url
    return client.get_providers()


def provider_get(cmd, client, base_url, provider):
    client._client.config.base_url = base_url
    return client.get_provider_by_id(provider)


def provider_deploy(cmd, client, base_url, provider, location=None, resource_group_name=None,  # pylint: disable=too-many-locals, too-many-statements
                    events=None, properties=None, version=None, prerelease=False, index_url=None, tags=None):
    from ._deploy_utils import (
        get_resource_group_by_name, create_resource_group_name, zip_deploy_app, get_arm_output,
        deploy_arm_template_at_resource_group, get_index_providers, open_url_in_browser)
    from .vendored_sdks.teamcloud.models import Provider, AzureResourceGroup

    client._client.config.base_url = base_url
    cli_ctx = cmd.cli_ctx

    hook = cli_ctx.get_progress_controller()
    hook.begin()

    hook.add(message='Fetching index.json from GitHub')
    version, zip_url, deploy_url, provider_name = get_index_providers(
        cli_ctx, provider, version, prerelease, index_url)

    resource_group_name = resource_group_name or provider_name

    hook.add(message='Getting resource group {}'.format(resource_group_name))
    rg, sub_id = get_resource_group_by_name(cli_ctx, resource_group_name)
    if rg is None:
        if location is None:
            raise CLIError(
                "--location/-l is required if resource group '{}' does not exist".format(resource_group_name))
        hook.add(message="Resource group '{}' not found".format(resource_group_name))
        hook.add(message="Creating resource group '{}'".format(resource_group_name))
        rg, sub_id = create_resource_group_name(cli_ctx, resource_group_name, location)

    hook.add(message='Deploying ARM template')
    outputs = deploy_arm_template_at_resource_group(
        cmd, resource_group_name, template_uri=deploy_url)

    name = get_arm_output(outputs, 'name')
    url = get_arm_output(outputs, 'url')
    auth_code = get_arm_output(outputs, 'authCode')
    principal_id = get_arm_output(outputs, 'principalId')
    setup_url = get_arm_output(outputs, 'setupUrl', raise_on_error=False)

    hook.add(message='Deploying provider source code')
    zip_deploy_app(cli_ctx, resource_group_name, name, zip_url)

    resource_group = AzureResourceGroup(
        id=rg.id, name=rg.name, region=rg.location, subscription_id=sub_id)
    payload = Provider(id=provider, url=url, auth_code=auth_code, principal_id=principal_id,
                       version=version, resource_group=resource_group, events=events,
                       properties=properties)

    provider_output = _create_with_status(cmd, client, base_url, payload,
                                          client.create_provider, hook_start=False)

    if setup_url:
        logger.warning('IMPORTANT: Opening a url in your browser to complete setup.')
        open_url_in_browser(setup_url)

    return provider_output


def provider_upgrade(cmd, client, base_url, provider, version=None, prerelease=False,  # pylint: disable=too-many-locals, too-many-statements
                     index_url=None):
    from ._deploy_utils import (
        get_resource_group_by_name, zip_deploy_app, get_index_providers, get_arm_output,
        deploy_arm_template_at_resource_group, open_url_in_browser, get_app_name)

    client._client.config.base_url = base_url
    cli_ctx = cmd.cli_ctx

    hook = cli_ctx.get_progress_controller()
    hook.begin()

    hook.add(message='Fetching index.json from GitHub')
    version, zip_url, deploy_url, _ = get_index_providers(
        cli_ctx, provider, version, prerelease, index_url)

    # get the provider from teamcloud
    hook.add(message='Getting existing provider')
    provider_result = client.get_provider_by_id(provider)
    existing_provider = provider_result.data

    if version and existing_provider.version and version == existing_provider.version:
        raise CLIError("Provider '{}' is already using version {}".format(provider, version))

    url = existing_provider.url
    name = get_app_name(url)

    resource_group_name = existing_provider.resource_group.name

    if not resource_group_name:
        raise CLIError('TODO provider was not deployed by the cli')

    hook.add(message='Getting resource group {}'.format(resource_group_name))
    rg, _ = get_resource_group_by_name(cli_ctx, resource_group_name)
    if rg is None:
        raise CLIError(
            "Resource Group '{}' must exist in current subscription.".format(resource_group_name))

    hook.add(message='Deploying ARM template')
    outputs = deploy_arm_template_at_resource_group(
        cmd, resource_group_name, template_uri=deploy_url)

    auth_code = get_arm_output(outputs, 'authCode')
    setup_url = get_arm_output(outputs, 'setupUrl', raise_on_error=False)

    hook.add(message='Deploying provider source code')
    zip_deploy_app(cli_ctx, resource_group_name, name, zip_url)

    payload = existing_provider
    payload.auth_code = auth_code
    payload.version = version

    provider_output = _update_with_status(cmd, client, base_url, payload,
                                          client.update_provider, hook_start=False)

    if setup_url:
        logger.warning('IMPORTANT: Opening a url in your browser to complete setup.')
        open_url_in_browser(setup_url)

    return provider_output


def provider_list_available(cmd, index_url=None, version=None, prerelease=False, show_details=False):
    from ._deploy_utils import get_index_providers_core

    version, index_providers = get_index_providers_core(cmd.cli_ctx, version, prerelease, index_url)

    if show_details:
        return index_providers or []

    if not index_providers:
        return []

    try:
        return [p for p in index_providers]
    except (AttributeError, ValueError):
        return []


def _create_with_status(cmd, client, base_url, payload, create_func,
                        project_id=None, no_wait=False, hook_start=True):
    from .vendored_sdks.teamcloud.models import StatusResult
    client._client.config.base_url = base_url

    if no_wait:
        return sdk_no_wait(no_wait, create_func, project_id, payload) if project_id else sdk_no_wait(
            no_wait, create_func, payload)

    type_name = create_func.metadata['url'].split('/')[-1][:-1].capitalize()

    hook = cmd.cli_ctx.get_progress_controller()
    if hook_start:
        hook.begin(message='Starting: Creating new {}'.format(type_name))
    hook.add(message='Starting: Creating new {}'.format(type_name))

    result = create_func(project_id, payload) if project_id else create_func(payload)

    while isinstance(result, StatusResult):
        if result.code == 200:
            hook.end(message=' ')
            logger.warning(' ')
            return result

        if result.code == 202:
            for _ in range(STATUS_POLLING_SLEEP_INTERVAL * 2):
                hook.add(message='{}: {}'.format(
                    result.state, result.state_message or 'Creating new {}'.format(type_name)))
                sleep(0.5)

            # status for project children
            if project_id:
                result = client.get_project_status(project_id, result._tracking_id)
            # status for project
            elif 'projects' in result.location:
                paths = urlparse(result.location).path.split('/')
                p_id = paths[paths.index('projects') + 1]
                result = client.get_project_status(p_id, result._tracking_id)
            # status for teamcloud children
            else:
                result = client.get_status(result._tracking_id)

    hook.end(message=' ')
    logger.warning(' ')

    return result


def _update_with_status(cmd, client, base_url, payload, update_func,
                        project_id=None, no_wait=False, hook_start=True):
    from .vendored_sdks.teamcloud.models import StatusResult
    client._client.config.base_url = base_url

    if no_wait:
        return sdk_no_wait(no_wait, update_func, project_id, payload) if project_id else sdk_no_wait(
            no_wait, update_func, payload)

    type_name = update_func.metadata['url'].split('/')[-1][:-1].capitalize()

    hook = cmd.cli_ctx.get_progress_controller()
    if hook_start:
        hook.begin(message='Starting: Updating {}'.format(type_name))
    hook.add(message='Starting: Updating {}'.format(type_name))

    result = update_func(project_id, payload) if project_id else update_func(payload)

    while isinstance(result, StatusResult):
        if result.code == 200:
            hook.end(message=' ')
            logger.warning(' ')
            return result

        if result.code == 202:
            for _ in range(STATUS_POLLING_SLEEP_INTERVAL * 2):
                hook.add(message='{}: {}'.format(
                    result.state, result.state_message or 'Updating {}'.format(type_name)))
                sleep(0.5)

            # status for project children
            if project_id:
                result = client.get_project_status(project_id, result._tracking_id)
            # status for project
            elif 'projects' in result.location:
                paths = urlparse(result.location).path.split('/')
                p_id = paths[paths.index('projects') + 1]
                result = client.get_project_status(p_id, result._tracking_id)
            # status for teamcloud children
            else:
                result = client.get_status(result._tracking_id)

    hook.end(message=' ')
    logger.warning(' ')

    return result


def _delete_with_status(cmd, client, base_url, item_id, delete_func,
                        project_id=None, no_wait=False, hook_start=True):
    from .vendored_sdks.teamcloud.models import StatusResult
    client._client.config.base_url = base_url

    if no_wait:
        return sdk_no_wait(no_wait, delete_func, item_id, project_id) if project_id else sdk_no_wait(
            no_wait, delete_func, item_id)

    type_name = delete_func.metadata['url'].split('/')[-2][:-1].capitalize()

    hook = cmd.cli_ctx.get_progress_controller()
    if hook_start:
        hook.begin(message='Starting: Deleting {}'.format(type_name))
    hook.add(message='Starting: Deleting {}'.format(type_name))

    result = delete_func(item_id, project_id) if project_id else delete_func(item_id)

    while isinstance(result, StatusResult):
        if result.code == 200:
            hook.end(message=' ')
            logger.warning(' ')
            return result

        if result.code == 202:
            for _ in range(STATUS_POLLING_SLEEP_INTERVAL * 2):
                hook.add(message='{}: {}'.format(
                    result.state, result.state_message or 'Deleting {}'.format(type_name)))
                sleep(0.5)

            # status for project children
            if project_id:
                result = client.get_project_status(project_id, result._tracking_id)
            # status for project
            elif 'projects' in result.location:
                paths = urlparse(result.location).path.split('/')
                p_id = paths[paths.index('projects') + 1]
                result = client.get_project_status(p_id, result._tracking_id)
            # status for teamcloud children
            else:
                result = client.get_status(result._tracking_id)

    hook.end(message=' ')
    logger.warning(' ')

    return result

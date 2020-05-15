# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------
# pylint: disable=unused-argument, protected-access, too-many-lines

from time import sleep
from urllib.parse import urlparse
from knack.util import CLIError
from knack.log import get_logger
from azure.cli.core.commands import LongRunningOperation
from azure.cli.core.profiles import ResourceType, get_sdk
from azure.cli.core.util import sdk_no_wait

logger = get_logger(__name__)

STATUS_POLLING_SLEEP_INTERVAL = 2


# TeamCloud

def teamcloud_deploy(cmd, client, name, location, resource_group_name='TeamCloud',  # pylint: disable=too-many-statements, too-many-locals
                     principal_name=None, principal_password=None, tags=None, version=None,
                     skip_app_deployment=False, skip_name_validation=False, skip_admin_user=False,
                     prerelease=False):
    from azure.cli.core._profile import Profile
    from .vendored_sdks.teamcloud.models import UserDefinition

    if version is None:
        version = _get_github_latest_release(cmd.cli_ctx, 'TeamCloud', prerelease=prerelease)

    cli_ctx = cmd.cli_ctx

    location = location.lower()

    logger.warning("Getting resource group '%s'...", resource_group_name)
    rg, subscription_id = _get_resource_group_by_name(cli_ctx, resource_group_name)
    if rg is None:
        logger.warning("Resource group '%s' not found.", resource_group_name)
        logger.warning("Creating resource group '%s'...", resource_group_name)
        _create_resource_group_name(cli_ctx, resource_group_name, location)

    name_short = ''
    for n in name.lower():
        if n.isalpha() or n.isdigit():
            name_short += n

    if len(name_short) > 14:
        name_short = name_short[:14]

    logger.warning('Creating app insights...')
    appinsights = _try_create_application_insights(
        cli_ctx, name_short + 'appinsights', resource_group_name, location)

    logger.warning('Creating deployment storage account...')
    dep_storage = _create_storage_account(
        cli_ctx, name_short + 'depstorage', resource_group_name, location, tags)

    logger.warning('Creating task hub storage account...')
    th_storage = _create_storage_account(
        cli_ctx, name_short + 'thtorage', resource_group_name, location, tags)

    logger.warning('Creating web jobs storage account...')
    wj_storage = _create_storage_account(
        cli_ctx, name_short + 'wjstorage', resource_group_name, location, tags)

    logger.warning('Creating cosmos db account. This may take several minutes to complete...')
    cosmosdb = _create_cosmosdb_account(
        cli_ctx, name_short + 'database', resource_group_name, location, tags)

    profile = Profile(cli_ctx=cli_ctx)

    if principal_name is None and principal_password is None:
        logger.warning('Creating aad app registration...')
        resource_manager_sp = _create_resource_manager_sp(cmd)
    else:
        _, _, tenant_id = profile.get_login_credentials(
            resource=cli_ctx.cloud.endpoints.active_directory_graph_resource_id)
        resource_manager_sp = {
            'appId': principal_name,
            'password': principal_password,
            'tenant': tenant_id
        }

    logger.warning('Creating app configuration service...')
    appconfig = _create_appconfig(cli_ctx, name + '-config', resource_group_name, location, tags)

    logger.warning('Adding resource info to app configuration service...')
    _set_appconfig_keys(cli_ctx, subscription_id, resource_manager_sp,
                        appconfig, cosmosdb, dep_storage)

    logger.warning('Creating orchestrator function app...')
    orchestrator, orchestrator_host_key = _create_function_app(
        cli_ctx, name + '-orchestrator', resource_group_name, location,
        wj_storage, th_storage, appconfig, appinsights, tags, with_identitiy=True)

    logger.warning('Adding orchestrator info to app configuration service...')
    _set_appconfig_orchestrator_keys(cli_ctx, subscription_id,
                                     appconfig, orchestrator, orchestrator_host_key)

    logger.warning('Creating api app service...')
    api_app = _create_api_app(cli_ctx, name, resource_group_name,
                              location, appconfig, appinsights, tags)

    logger.warning('Successfully deployed Azure resources for TeamCloud')
    base_url = 'https://{}'.format(api_app.default_host_name)

    if skip_app_deployment:
        logger.warning(
            'IMPORTANT: --skip-deploy prevented source code for the TeamCloud instance deployment. '
            'To deploy the applications use `az tc upgrade`.')
    else:
        logger.warning('Deploying orchestrator source code...')
        _zip_deploy_app(cli_ctx, resource_group_name, name + '-orchestrator', 'https://github.com/microsoft/TeamCloud',
                        'TeamCloud.Orchestrator', version=version, app_instance=orchestrator)

        logger.warning('Deploying api app source code...')
        _zip_deploy_app(cli_ctx, resource_group_name, name, 'https://github.com/microsoft/TeamCloud',
                        'TeamCloud.API', version=version, app_instance=api_app)

        logger.warning('Successfully created TeamCloud instance.')

    if skip_admin_user:
        logger.warning(
            'IMPORTANT: --redeploy prevented adding you as an Admin user to the TeamCloud instance deployment.')
    else:
        logger.warning('Creating admin user...')
        me = profile.get_current_account_user()

        client._client.config.base_url = base_url
        user_definition = UserDefinition(email=me, role='Admin', tags=None)
        _ = client.create_team_cloud_admin_user(user_definition)

    logger.warning('TeamCloud instance successfully created at: %s', base_url)
    logger.warning('Use `az configure --defaults tc-base-url=%s` to configure '
                   'this as your default TeamCloud instance', base_url)

    result = {
        'deployed': not skip_app_deployment,
        'version': version or 'latest',
        'name': name,
        'base_url': base_url,
        'location': location,
        'api': {
            'name': name,
            'url': 'https://{}'.format(api_app.default_host_name)
        },
        'orchestrator': {
            'name': name + '-orchestrator',
            'url': 'https://{}'.format(orchestrator.default_host_name)
        },
        'service_principal': {
            'appId': resource_manager_sp['appId'],
            # 'password': resource_manager_sp['password'],
            'tenant': resource_manager_sp['tenant']
        }
    }

    return result


def teamcloud_upgrade(cmd, client, base_url, resource_group_name='TeamCloud', version=None, prerelease=False):
    from re import match

    if version is None:
        version = _get_github_latest_release(cmd.cli_ctx, 'TeamCloud', prerelease=prerelease)

    logger.warning("Getting resource group '%s'...", resource_group_name)
    rg, _ = _get_resource_group_by_name(cmd.cli_ctx, resource_group_name)
    if rg is None:
        logger.warning("Resource group '%s' not found.", resource_group_name)
        raise CLIError(
            "Resource group '{}' must exist in current subscription.".format(resource_group_name))

    name = ''
    m = match(r'^https?://(?P<name>[a-zA-Z0-9-]+)\.azurewebsites\.net[/a-zA-Z0-9.\:]*$', base_url)
    try:
        name = m.group('name') if m is not None else None
    except IndexError:
        pass

    if name is None or '':
        raise CLIError("Unable to get app name from base url.")

    logger.warning('Deploying orchestrator source code (version: %s)...', version)
    _zip_deploy_app(cmd.cli_ctx, resource_group_name, name + '-orchestrator',
                    'https://github.com/microsoft/TeamCloud', 'TeamCloud.Orchestrator', version=version)

    logger.warning('Deploying api app source code (version: %s)...', version)
    _zip_deploy_app(cmd.cli_ctx, resource_group_name, name,
                    'https://github.com/microsoft/TeamCloud', 'TeamCloud.API', version=version)

    version_string = version or 'the latest version'
    logger.warning("TeamCloud instance '%s' was successfully upgraded to %s.", name, version_string)

    result = {
        'version': version or 'latest',
        'name': name,
        'base_url': '{}'.format(base_url),
        'api': {
            'name': name
        },
        'orchestrator': {
            'name': name + '-orchestrator'
        }
    }

    return result


def status_get(cmd, client, base_url, tracking_id, project=None):
    client._client.config.base_url = base_url
    return client.get_project_status(project, tracking_id) if project else client.get_status(tracking_id)


# TeamCloud Users

def teamcloud_user_create(cmd, client, base_url, user_name, user_role='Creator', tags=None, no_wait=False):
    from .vendored_sdks.teamcloud.models import UserDefinition

    payload = UserDefinition(email=user_name, role=user_role, tags=tags)

    return sdk_no_wait(no_wait, _create_with_status, cmd, client, base_url,
                       payload, client.create_team_cloud_user)


def teamcloud_user_delete(cmd, client, base_url, user, no_wait=False):
    return sdk_no_wait(no_wait, _delete_with_status, cmd, client, base_url,
                       user, client.delete_team_cloud_user)


def teamcloud_user_list(cmd, client, base_url):
    client._client.config.base_url = base_url
    return client.get_team_cloud_users()


def teamcloud_user_get(cmd, client, base_url, user):
    client._client.config.base_url = base_url
    return client.get_team_cloud_user_by_name_or_id(user)


# TeamCloud Tags

def teamcloud_tag_create(cmd, client, base_url, tag_key, tag_value, no_wait=False):
    payload = {tag_key, tag_value}

    return sdk_no_wait(no_wait, _create_with_status, cmd, client, base_url,
                       payload, client.create_team_cloud_tag)


def teamcloud_tag_delete(cmd, client, base_url, tag_key, no_wait=False):
    return sdk_no_wait(no_wait, _delete_with_status, cmd, client, base_url,
                       tag_key, client.delete_team_cloud_tag)


def teamcloud_tag_list(cmd, client, base_url):
    client._client.config.base_url = base_url
    return client.get_team_cloud_tags()


def teamcloud_tag_get(cmd, client, base_url, tag_key):
    client._client.config.base_url = base_url
    return client.get_team_cloud_tag_by_key(tag_key)


# Projects

def project_create(cmd, client, base_url, name, project_type=None, tags=None, no_wait=False):
    from .vendored_sdks.teamcloud.models import ProjectDefinition

    payload = ProjectDefinition(name=name, project_type=project_type, tags=tags)

    return sdk_no_wait(no_wait, _create_with_status, cmd, client, base_url,
                       payload, client.create_project)


def project_delete(cmd, client, base_url, project, no_wait=False):
    return sdk_no_wait(no_wait, _delete_with_status, cmd, client, base_url,
                       project, client.delete_project)


def project_list(cmd, client, base_url):
    client._client.config.base_url = base_url
    return client.get_projects()


def project_get(cmd, client, base_url, project):
    client._client.config.base_url = base_url
    return client.get_project_by_name_or_id(project)


# Project Users

def project_user_create(cmd, client, base_url, project, user_name, user_role='Member', tags=None, no_wait=False):
    from .vendored_sdks.teamcloud.models import UserDefinition

    payload = UserDefinition(email=user_name, role=user_role, tags=tags)

    return sdk_no_wait(no_wait, _create_with_status, cmd, client, base_url,
                       payload, client.create_project_user, project_id=project)


def project_user_delete(cmd, client, base_url, project, user, no_wait=False):
    return sdk_no_wait(no_wait, _delete_with_status, cmd, client, base_url,
                       user, client.delete_project_user, project_id=project)


def project_user_list(cmd, client, base_url, project):
    client._client.config.base_url = base_url
    return client.get_project_users(project)


def project_user_get(cmd, client, base_url, project, user):
    client._client.config.base_url = base_url
    return client.get_project_user_by_name_or_id(user, project)


# Project Tags

def project_tag_create(cmd, client, base_url, project, tag_key, tag_value, no_wait=False):
    payload = {tag_key, tag_value}

    return sdk_no_wait(no_wait, _create_with_status, cmd, client, base_url,
                       payload, client.create_project_tag, project_id=project)


def project_tag_delete(cmd, client, base_url, project, tag_key, no_wait=False):
    return sdk_no_wait(no_wait, _delete_with_status, cmd, client, base_url,
                       tag_key, client.delete_project_tag, project_id=project)


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

    payload = ProjectType(id=project_type, default=default, region=location,
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

    return sdk_no_wait(no_wait, _create_with_status, cmd, client, base_url,
                       payload, client.create_provider)


def provider_delete(cmd, client, base_url, provider, no_wait=False):
    return sdk_no_wait(no_wait, _delete_with_status, cmd, client, base_url,
                       provider, client.delete_provider)


def provider_list(cmd, client, base_url):
    client._client.config.base_url = base_url
    return client.get_providers()


def provider_get(cmd, client, base_url, provider):
    client._client.config.base_url = base_url
    return client.get_provider_by_id(provider)


def provider_deploy(cmd, client, base_url, provider, location, resource_group_name=None,
                    events=None, properties=None, version=None, prerelease=False, tags=None):
    from azure.cli.core.util import random_string
    client._client.config.base_url = base_url
    cli_ctx = cmd.cli_ctx

    zip_name = None
    if provider == 'azure.appinsights':
        zip_name = 'TeamCloud.Providers.Azure.AppInsights'
    if provider == 'azure.devops':
        zip_name = 'TeamCloud.Providers.Azure.DevOps'
    if provider == 'azure.devtestlabs':
        zip_name = 'TeamCloud.Providers.Azure.DevTestLabs'

    if zip_name is None:
        raise CLIError(
            "--provider is invalid.  Must be one of 'azure.appinsights', 'azure.devops', 'azure.devtestlabs'")

    if resource_group_name is None:
        resource_group_name = zip_name

    if version is None:
        version = _get_github_latest_release(
            cmd.cli_ctx, 'TeamCloud-Providers', prerelease=prerelease)

    logger.warning("Getting resource group '%s'...", resource_group_name)
    rg, _ = _get_resource_group_by_name(cli_ctx, resource_group_name)
    if rg is None:
        logger.warning("Resource group '%s' not found.", resource_group_name)
        logger.warning("Creating resource group '%s'...", resource_group_name)
        _create_resource_group_name(cli_ctx, resource_group_name, location)

    name = random_string(length=14, force_lower=True)

    logger.warning('Creating task hub storage account...')
    th_storage = _create_storage_account(cli_ctx, name + 'thtorage', resource_group_name, location)

    logger.warning('Creating web jobs storage account...')
    wj_storage = _create_storage_account(cli_ctx, name + 'wjstorage', resource_group_name, location)

    logger.warning('Creating provider function app...')
    functionapp, host_key = _create_function_app(
        cli_ctx, name, resource_group_name, location, wj_storage, th_storage, tags=tags, with_identitiy=True)

    url = 'https://{}'.format(functionapp.default_host_name)

    logger.warning('Deploying provider source code (version: %s)...', version)
    _zip_deploy_app(cli_ctx, resource_group_name, name, 'https://github.com/microsoft/TeamCloud-Providers',
                    zip_name, version=version, app_instance=functionapp)

    return provider_create(cmd, client, base_url, provider, url, host_key, events, properties)


def provider_upgrade(cmd, client, base_url, provider, resource_group_name=None, version=None, prerelease=False):
    from re import match
    client._client.config.base_url = base_url
    cli_ctx = cmd.cli_ctx

    zip_name = None
    if provider == 'azure.appinsights':
        zip_name = 'TeamCloud.Providers.Azure.AppInsights'
    if provider == 'azure.devops':
        zip_name = 'TeamCloud.Providers.Azure.DevOps'
    if provider == 'azure.devtestlabs':
        zip_name = 'TeamCloud.Providers.Azure.DevTestLabs'

    if zip_name is None:
        raise CLIError(
            "--provider is invalid.  Must be one of 'azure.appinsights', 'azure.devops', 'azure.devtestlabs'")

    if resource_group_name is None:
        resource_group_name = zip_name

    if version is None:
        version = _get_github_latest_release(
            cmd.cli_ctx, 'TeamCloud-Providers', prerelease=prerelease)

    logger.warning("Getting resource group '%s'...", resource_group_name)
    rg, _ = _get_resource_group_by_name(cli_ctx, resource_group_name)
    if rg is None:
        logger.warning("Resource group '%s' not found.", resource_group_name)
        raise CLIError(
            "Resource group '{}' must exist in current subscription.".format(resource_group_name))

    provider_result = client.get_provider_by_id(provider)

    url = provider_result.data.url

    name = ''
    m = match(r'^https?://(?P<name>[a-zA-Z0-9-]+)\.azurewebsites\.net[/a-zA-Z0-9.\:]*$', url)
    try:
        name = m.group('name') if m is not None else None
    except IndexError:
        pass

    if name is None or '':
        raise CLIError('Unable to function app name from provider url.')

    logger.warning('Deploying provider source code (version: %s)...', version)
    _zip_deploy_app(cli_ctx, resource_group_name, name,
                    'https://github.com/microsoft/TeamCloud-Providers', zip_name, version=version)

    version_string = version or 'the latest version'
    logger.warning("Provider '%s' was successfully upgraded to %s.", name, version_string)

    return provider_result.data


# Util

def _create_with_status(cmd, client, base_url, payload, create_func, project_id=None):
    from .vendored_sdks.teamcloud.models import StatusResult
    client._client.config.base_url = base_url

    type_name = create_func.metadata['url'].split('/')[-1][:-1].capitalize()

    hook = cmd.cli_ctx.get_progress_controller()

    hook.add(message='Starting: Creating new {}'.format(type_name))

    result = create_func(project_id, payload) if project_id else create_func(payload)

    while isinstance(result, StatusResult):
        if result.code == 200:
            hook.end(message='Finished.')
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

    hook.end(message='Finished.')

    return result


def _delete_with_status(cmd, client, base_url, item_id, delete_func, project_id=None, **kwargs):
    from .vendored_sdks.teamcloud.models import StatusResult
    client._client.config.base_url = base_url

    type_name = delete_func.metadata['url'].split('/')[-2][:-1].capitalize()

    hook = cmd.cli_ctx.get_progress_controller()

    hook.add(message='Starting: Delete {}'.format(type_name))

    result = delete_func(item_id, project_id) if project_id else delete_func(item_id)

    while isinstance(result, StatusResult):
        if result.code == 200:
            hook.end(messag='Finished.')
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

    hook.end(messag='Finished.')

    return result


def _get_resource_group_by_name(cli_ctx, resource_group_name):
    from ._client_factory import resource_client_factory

    try:
        resouce_client = resource_client_factory(cli_ctx).resource_groups
        return resouce_client.get(resource_group_name), resouce_client.config.subscription_id
    except Exception as ex:  # pylint: disable=broad-except
        error = getattr(ex, 'Azure Error', ex)
        if error != 'ResourceGroupNotFound':
            return None, resouce_client.config.subscription_id
        raise


def _create_resource_group_name(cli_ctx, resource_group_name, location, tags=None):
    from ._client_factory import resource_client_factory

    ResourceGroup = get_sdk(cli_ctx, ResourceType.MGMT_RESOURCE_RESOURCES,
                            'ResourceGroup', mod='models')
    resource_client = resource_client_factory(cli_ctx).resource_groups
    parameters = ResourceGroup(location=location, tags=tags)
    resource_client.create_or_update(resource_group_name, parameters)


def _create_storage_account(cli_ctx, name, resource_group_name, location, tags=None):
    from ._client_factory import storage_client_factory
    from azure.mgmt.storage.models import Sku, SkuName, StorageAccountCreateParameters

    params = StorageAccountCreateParameters(sku=Sku(name=SkuName.standard_ragrs),
                                            kind='StorageV2', location=location, tags=tags)

    storage_client = storage_client_factory(cli_ctx).storage_accounts
    LongRunningOperation(cli_ctx)(storage_client.create(resource_group_name, name, params))

    properties = storage_client.get_properties(resource_group_name, name)
    keys = storage_client.list_keys(resource_group_name, name)

    endpoint_suffix = cli_ctx.cloud.suffixes.storage_endpoint
    connection_string = 'DefaultEndpointsProtocol={};EndpointSuffix={};AccountName={};AccountKey={}'.format(
        "https", endpoint_suffix, name, keys.keys[0].value)  # pylint: disable=no-member

    return properties, keys, connection_string


def _create_cosmosdb_account(cli_ctx, name, resource_group_name, location, tags=None):
    from ._client_factory import cosmosdb_client_factory
    from azure.mgmt.cosmosdb.models import DatabaseAccountKind, Location, DatabaseAccountCreateUpdateParameters

    locations = []
    locations.append(Location(location_name=location, failover_priority=0, is_zone_redundant=False))
    params = DatabaseAccountCreateUpdateParameters(
        location=location, locations=locations, tags=tags, kind=DatabaseAccountKind.global_document_db.value)

    cosmos_client = cosmosdb_client_factory(cli_ctx).database_accounts

    async_docdb_create = cosmos_client.create_or_update(resource_group_name, name, params)
    docdb_account = async_docdb_create.result()
    docdb_account = cosmos_client.get(resource_group_name, name)  # Workaround

    connection_strings = cosmos_client.list_connection_strings(resource_group_name, name)

    return docdb_account, connection_strings.connection_strings[0].connection_string


def _create_appconfig(cli_ctx, name, resource_group_name, location, tags=None):
    from ._client_factory import appconfig_client_factory
    from azure.mgmt.appconfiguration.models import ConfigurationStore, Sku

    params = ConfigurationStore(location=location.lower(), identity=None,
                                sku=Sku(name='Standard'), tags=tags)

    appconfig_client = appconfig_client_factory(cli_ctx).configuration_stores

    LongRunningOperation(cli_ctx)(appconfig_client.create(resource_group_name, name, params))

    appconfig = appconfig_client.get(resource_group_name, name)
    keys = appconfig_client.list_keys(resource_group_name, name)

    key = next((k for k in keys if not k.read_only), None)

    return appconfig, keys, key.connection_string


def _set_appconfig_keys(cli_ctx, subscription_id, resource_manager_sp, appconfig, cosmosdb, dep_storage):
    from azure.cli.command_modules.appconfig._azconfig.azconfig_client import AzconfigClient
    from azure.cli.command_modules.appconfig._azconfig.models import KeyValue

    azconfig_client = AzconfigClient(appconfig[2])

    tenant_id = resource_manager_sp['tenant']

    set_kvs = []

    set_kvs.append(KeyValue(key='Azure:SubscriptionId', value=subscription_id))
    set_kvs.append(KeyValue(key='Azure:TenantId', value=tenant_id))
    set_kvs.append(KeyValue(key='Azure:ResourceManager:ClientId',
                            value=resource_manager_sp['appId']))
    set_kvs.append(KeyValue(key='Azure:ResourceManager:ClientSecret',
                            value=resource_manager_sp['password']))
    set_kvs.append(KeyValue(key='Azure:ResourceManager:TenantId', value=tenant_id))
    set_kvs.append(KeyValue(key='Azure:CosmosDb:ConnectionString', value=cosmosdb[1]))
    set_kvs.append(KeyValue(key='Azure:DeploymentStorage:ConnectionString', value=dep_storage[2]))

    for set_kv in set_kvs:
        azconfig_client.set_keyvalue(set_kv)


def _set_appconfig_orchestrator_keys(cli_ctx, subscription_id, appconfig, orchestrator, orchestrator_host_key):
    from azure.cli.command_modules.appconfig._azconfig.azconfig_client import AzconfigClient
    from azure.cli.command_modules.appconfig._azconfig.models import KeyValue

    azconfig_client = AzconfigClient(appconfig[2])

    set_kvs = []
    set_kvs.append(KeyValue(key='Orchestrator:Url',
                            value='https://{}'.format(orchestrator.default_host_name)))
    set_kvs.append(KeyValue(key='Orchestrator:AuthCode', value=orchestrator_host_key))

    for set_kv in set_kvs:
        azconfig_client.set_keyvalue(set_kv)


def _create_api_app(cli_ctx, name, resource_group_name, location, appconfig, app_insights, tags=None):
    from ._client_factory import web_client_factory

    SkuDescription, AppServicePlan, SiteConfig, Site, NameValuePair, ConnStringInfo = get_sdk(
        cli_ctx, ResourceType.MGMT_APPSERVICE, 'SkuDescription', 'AppServicePlan', 'SiteConfig',
        'Site', 'NameValuePair', 'ConnStringInfo', mod='models')

    web_client = web_client_factory(cli_ctx)

    sku_def = SkuDescription(tier='STANDARD', name='S1', capacity=None)
    plan_def = AppServicePlan(location=location, tags=tags, sku=sku_def,
                              reserved=None, hyper_v=None, name=name,
                              per_site_scaling=False, hosting_environment_profile=None)

    app_service_poller = web_client.app_service_plans.create_or_update(
        name=name, resource_group_name=resource_group_name, app_service_plan=plan_def)
    app_service = LongRunningOperation(cli_ctx)(app_service_poller)

    site_config = SiteConfig(app_settings=[], connection_strings=[])
    site_config.always_on = True

    site_config.connection_strings.append(ConnStringInfo(
        name='ConfigurationService', connection_string=appconfig[2]))

    site_config.app_settings.append(NameValuePair(name="WEBSITE_NODE_DEFAULT_VERSION",
                                                  value='10.14'))
    site_config.app_settings.append(NameValuePair(name='ANCM_ADDITIONAL_ERROR_PAGE_LINK',
                                                  value='https://{}.scm.azurewebsites.net/detectors'.format(name)))
    site_config.app_settings.append(NameValuePair(name='ApplicationInsightsAgent_EXTENSION_VERSION',
                                                  value='~2'))

    if app_insights is not None and app_insights.instrumentation_key is not None:
        site_config.app_settings.append(NameValuePair(name='APPINSIGHTS_INSTRUMENTATIONKEY',
                                                      value=app_insights.instrumentation_key))

    webapp_def = Site(location=location, site_config=site_config,
                      server_farm_id=app_service.id, tags=tags)

    poller = web_client.web_apps.create_or_update(resource_group_name, name, webapp_def)
    webapp = LongRunningOperation(cli_ctx)(poller)

    return webapp


def _create_function_app(cli_ctx, name, resource_group_name, location, wj_storage, th_storage,  # pylint: disable=too-many-locals
                         appconfig=None, app_insights=None, tags=None, with_identitiy=False):
    from ._client_factory import web_client_factory
    from azure.cli.core.util import send_raw_request

    SiteConfig, Site, NameValuePair, ConnStringInfo, ManagedServiceIdentity = get_sdk(
        cli_ctx, ResourceType.MGMT_APPSERVICE, 'SiteConfig', 'Site', 'NameValuePair', 'ConnStringInfo',
        'ManagedServiceIdentity', mod='models')

    web_client = web_client_factory(cli_ctx)

    site_config = SiteConfig(app_settings=[], connection_strings=[])

    regions = web_client.list_geo_regions(sku='Dynamic')
    locations = [{'name': x.name.lower().replace(' ', '')} for x in regions]

    deploy_location = next((for l in locations if l['name'].lower() == location.lower()), None)
    if deploy_location is None:
        raise CLIError('Location is invalid. Use: az functionapp list-consumption-locations')

    if appconfig is not None:
        site_config.connection_strings.append(ConnStringInfo(name='ConfigurationService',
                                                             connection_string=appconfig[2]))

    # adding appsetting to site to make it a function
    site_config.app_settings.append(NameValuePair(name='FUNCTIONS_EXTENSION_VERSION', value='~3'))
    site_config.app_settings.append(NameValuePair(name='AzureWebJobsStorage', value=wj_storage[2]))
    site_config.app_settings.append(NameValuePair(name='DurableFunctionsHubStorage',
                                                  value=th_storage[2]))
    site_config.app_settings.append(NameValuePair(name='WEBSITE_NODE_DEFAULT_VERSION', value='~12'))
    site_config.app_settings.append(NameValuePair(name='WEBSITE_CONTENTAZUREFILECONNECTIONSTRING',
                                                  value=wj_storage[2]))
    site_config.app_settings.append(NameValuePair(name='WEBSITE_CONTENTSHARE', value=name.lower()))
    site_config.app_settings.append(NameValuePair(name='FUNCTION_APP_EDIT_MODE', value='readonly'))

    if app_insights is not None and app_insights.instrumentation_key is not None:
        site_config.app_settings.append(NameValuePair(name='APPINSIGHTS_INSTRUMENTATIONKEY',
                                                      value=app_insights.instrumentation_key))

    functionapp_def = Site(location=None, site_config=site_config, tags=tags)
    functionapp_def.location = location
    functionapp_def.kind = 'functionapp'

    if with_identitiy:
        functionapp_def.identity = ManagedServiceIdentity(type='SystemAssigned')

    poller = web_client.web_apps.create_or_update(resource_group_name, name, functionapp_def)
    functionapp = LongRunningOperation(cli_ctx)(poller)

    if with_identitiy:
        def getter():
            return functionapp

        def setter(webapp):
            return webapp

        from azure.cli.core.commands.arm import assign_identity as _assign_identity
        functionapp = _assign_identity(cli_ctx, getter, setter, 'Contributor', functionapp.id)

    admin_token = web_client.web_apps.get_functions_admin_token(resource_group_name, name)

    host_key_url = 'https://{}/admin/host/keys/default'.format(functionapp.default_host_name)
    host_key_auth_header = 'Authorization=Bearer {}'.format(admin_token)

    host_key_response = send_raw_request(cli_ctx, 'GET', host_key_url, [host_key_auth_header],
                                         skip_authorization_header=True)
    host_key_json = host_key_response.json()
    host_key = host_key_json['value']

    return functionapp, host_key


def _try_create_application_insights(cli_ctx, name, resource_group_name, location):
    from azure.cli.core.commands.client_factory import get_mgmt_service_client
    from azure.mgmt.applicationinsights import ApplicationInsightsManagementClient

    creation_failed_warn = 'Unable to create the Application Insights for the TeamCloud instance. ' \
                           'Please use the Azure Portal to manually create and configure the ' \
                           'Application Insights, if needed.'

    app_insights_client = get_mgmt_service_client(cli_ctx, ApplicationInsightsManagementClient)
    properties = {
        'name': name,
        'location': location,
        'kind': 'web',
        'properties': {
            'Application_Type': 'web'
        }
    }

    appinsights = app_insights_client.components.create_or_update(resource_group_name,
                                                                  name, properties)

    if appinsights is None or appinsights.instrumentation_key is None:
        logger.warning(creation_failed_warn)
        return None

    # We make this success message as a warning to no interfere with regular JSON output in stdout
    logger.warning('Application Insights \"%s\" was created.', appinsights.name)
    logger.warning('View the Application Insights component at '
                   'https://portal.azure.com/#resource%s/overview', appinsights.id)

    return appinsights


def _create_keyvault(cli_ctx, name, resource_group_name, location):
    pass


def _create_resource_manager_sp(cmd):
    from azure.cli.command_modules.role.custom import create_service_principal_for_rbac, add_permission, admin_consent

    sp = create_service_principal_for_rbac(cmd, name='http://TeamCloud.ResourceManager',
                                           years=10, role='Owner')
    # Azure Active Directory Graph permissions
    add_permission(cmd, identifier=sp['appId'], api='00000002-0000-0000-c000-000000000000',
                   api_permissions=['5778995a-e1bf-45b8-affa-663a9f3f4d04=Role',  # Directory.Read.All
                                    '824c81eb-e3f8-4ee6-8f6d-de7f50d565b7=Role'])  # Application.ReadWrite.OwnedBy
    # Microsoft Graph permissions
    add_permission(cmd, identifier=sp['appId'], api='00000003-0000-0000-c000-000000000000',
                   api_permissions=['7ab1d382-f21e-4acd-a863-ba3e13f7da61=Role',  # Directory.Read.All
                                    '18a4783c-866b-4cc7-a460-3d5e5662c884=Role'])  # Application.ReadWrite.OwnedBy

    # 'e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope', # User.Read
    # 'df021288-bdef-4463-88db-98f22de89214=Role', # User.Read

    admin_consent(cmd, identifier=sp['appId'])

    return sp


def _zip_deploy_app(cli_ctx, resource_group_name, name, repo_url, zip_name, version=None, slot=None,
                    app_instance=None, timeout=None):
    import requests
    import urllib3

    from azure.cli.core.util import should_disable_connection_verify
    from ._client_factory import web_client_factory

    web_client = web_client_factory(cli_ctx).web_apps

    #  work around until the timeout limits issue for linux is investigated & fixed
    creds = web_client.list_publishing_credentials(resource_group_name, name)
    creds = creds.result()

    try:
        scm_url = _get_scm_url(cli_ctx, resource_group_name, name,
                               slot=slot, app_instance=app_instance)
    except ValueError:
        raise CLIError('Failed to fetch scm url for azure app service app')

    zipdeploy_url = scm_url + '/api/zipdeploy?isAsync=true'
    deployment_status_url = scm_url + '/api/deployments/latest'

    authorization = urllib3.util.make_headers(basic_auth='{}:{}'.format(
        creds.publishing_user_name, creds.publishing_password))

    zip_package_uri = '{}/releases/latest/download/{}.zip'.format(repo_url, zip_name)
    if version:
        zip_package_uri = '{}/releases/download/{}/{}.zip'.format(repo_url, version, zip_name)

    logger.warning('Starting zip deployment. This may take several minutes to complete...')
    res = requests.put(zipdeploy_url, headers=authorization,
                       json={'packageUri': zip_package_uri}, verify=not should_disable_connection_verify())

    # check if there's an ongoing process
    if res.status_code == 409:
        raise CLIError('There may be an ongoing deployment or your app setting has WEBSITE_RUN_FROM_PACKAGE. '
                       'Please track your deployment in {} and ensure the WEBSITE_RUN_FROM_PACKAGE app setting '
                       'is removed.'.format(deployment_status_url))

    # check the status of async deployment
    response = _check_zip_deployment_status(cli_ctx, resource_group_name, name, deployment_status_url,
                                            authorization, slot=slot, app_instance=app_instance, timeout=timeout)

    return response


def _check_zip_deployment_status(cli_ctx, resource_group_name, name, deployment_status_url,
                                 authorization, slot=None, app_instance=None, timeout=None):
    import json
    import requests
    from azure.cli.core.util import should_disable_connection_verify

    total_trials = (int(timeout) // 2) if timeout else 450
    num_trials = 0

    while num_trials < total_trials:
        sleep(2)
        response = requests.get(deployment_status_url, headers=authorization,
                                verify=not should_disable_connection_verify())
        sleep(2)
        try:
            res_dict = response.json()
        except json.decoder.JSONDecodeError:
            logger.warning("Deployment status endpoint %s returned malformed data. Retrying...",
                           deployment_status_url)
            res_dict = {}
        finally:
            num_trials = num_trials + 1

        if res_dict.get('status', 0) == 3:
            _configure_default_logging(cli_ctx, resource_group_name, name,
                                       slot=slot, app_instance=app_instance)
            raise CLIError('Zip deployment failed. {}. Please run the command az webapp log tail -n {} -g {}'.format(
                res_dict, name, resource_group_name))
        if res_dict.get('status', 0) == 4:
            break
        if 'progress' in res_dict:
            # show only in debug mode, customers seem to find this confusing
            logger.info(res_dict['progress'])
    # if the deployment is taking longer than expected
    if res_dict.get('status', 0) != 4:
        _configure_default_logging(cli_ctx, resource_group_name, name,
                                   slot=slot, app_instance=app_instance)
        raise CLIError(
            'Timeout reached by the command, however, the deployment operation is still on-going. '
            'Navigate to your scm site to check the deployment status')
    return res_dict


# TODO: expose new blob suport
def _configure_default_logging(cli_ctx, resource_group_name, name, slot=None, app_instance=None, level=None,
                               web_server_logging='filesystem', docker_container_logging='true'):
    from azure.mgmt.web.models import (FileSystemApplicationLogsConfig, ApplicationLogsConfig,
                                       SiteLogsConfig, HttpLogsConfig, FileSystemHttpLogsConfig)

    logger.warning('Configuring default logging for the app, if not already enabled...')

    site = _get_webapp(cli_ctx, resource_group_name, name, slot=slot, app_instance=app_instance)

    location = site.location

    fs_log = FileSystemApplicationLogsConfig(level='Error')
    application_logs = ApplicationLogsConfig(file_system=fs_log)

    http_logs = None
    server_logging_option = web_server_logging or docker_container_logging
    if server_logging_option:
        # TODO: az blob storage log config currently not in use, will be impelemented later.
        # Tracked as Issue: #4764 on Github
        filesystem_log_config = None
        turned_on = server_logging_option != 'off'
        if server_logging_option in ['filesystem', 'off']:
            # 100 mb max log size, retention lasts 3 days. Yes we hard code it, portal does too
            filesystem_log_config = FileSystemHttpLogsConfig(
                retention_in_mb=100, retention_in_days=3, enabled=turned_on)

        http_logs = HttpLogsConfig(file_system=filesystem_log_config, azure_blob_storage=None)

    site_log_config = SiteLogsConfig(location=location, application_logs=application_logs,
                                     http_logs=http_logs, failed_requests_tracing=None,
                                     detailed_error_messages=None)

    from ._client_factory import web_client_factory
    web_client = web_client_factory(cli_ctx).web_apps

    return web_client.update_diagnostic_logs_config(resource_group_name, name, site_log_config)


def _get_scm_url(cli_ctx, resource_group_name, name, slot=None, app_instance=None):  # pylint: disable=inconsistent-return-statements
    from azure.mgmt.web.models import HostType

    webapp = _get_webapp(cli_ctx, resource_group_name, name, slot=slot, app_instance=app_instance)
    for host in webapp.host_name_ssl_states or []:
        if host.host_type == HostType.repository:
            return 'https://{}'.format(host.name)


def _get_webapp(cli_ctx, resource_group_name, name, slot=None, app_instance=None):
    webapp = app_instance
    if not app_instance:
        from ._client_factory import web_client_factory
        web_client = web_client_factory(cli_ctx).web_apps
        webapp = web_client.get(resource_group_name, name)
    if not webapp:
        raise CLIError("'{}' app doesn't exist".format(name))

    # Should be renamed in SDK in a future release
    try:
        setattr(webapp, 'app_service_plan_id', webapp.server_farm_id)
        del webapp.server_farm_id
    except AttributeError:
        pass

    return webapp


def _get_github_latest_release(cli_ctx, repo, org='microsoft', prerelease=False):
    import requests
    from azure.cli.core.util import should_disable_connection_verify

    url = 'https://api.github.com/repos/{}/{}/releases'.format(org, repo)

    if prerelease:
        version_res = requests.get(url, verify=not should_disable_connection_verify())
        version_json = version_res.json()

        version_prerelease = next((v for v in version_json if v['prerelease']), None)
        if not version_prerelease:
            raise CLIError('--pre no prerelease versions found for {}/{}'.format(org, repo))

        return version_prerelease['tag_name']

    url = url + '/latest'
    version_res = requests.get(url, verify=not should_disable_connection_verify())

    if version_res.status_code == 404:
        raise CLIError(
            'No release version exists for {}/{}. '
            'Specify a specific prerelease version with --version '
            'or use latest prerelease with --pre'.format(org, repo))

    version_json = version_res.json()
    return version_json['tag_name']

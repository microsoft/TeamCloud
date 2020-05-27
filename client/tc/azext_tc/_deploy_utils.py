# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------
# pylint: disable=unused-argument, protected-access, too-many-lines

from knack.util import CLIError
from knack.log import get_logger
from azure.cli.core.commands import LongRunningOperation
from azure.cli.core.profiles import ResourceType, get_sdk
from azure.cli.core.util import (
    can_launch_browser, open_page_in_browser, in_cloud_console, should_disable_connection_verify)


ERR_TMPL_PRDR_INDEX = 'Unable to get provider index.\n'
ERR_TMPL_NON_200 = '{}Server returned status code {{}} for {{}}'.format(ERR_TMPL_PRDR_INDEX)
ERR_TMPL_NO_NETWORK = '{}Please ensure you have network connection. Error detail: {{}}'.format(
    ERR_TMPL_PRDR_INDEX)
ERR_TMPL_BAD_JSON = '{}Response body does not contain valid json. Error detail: {{}}'.format(
    ERR_TMPL_PRDR_INDEX)

ERR_UNABLE_TO_GET_PROVIDERS = 'Unable to get providers from index. Improper index format.'
ERR_UNABLE_TO_GET_TEAMCLOUD = 'Unable to get teamcloud from index. Improper index format.'

TRIES = 3

logger = get_logger(__name__)


def get_github_latest_release(cli_ctx, repo, org='microsoft', prerelease=False):
    import requests

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


def get_resource_group_by_name(cli_ctx, resource_group_name):
    from ._client_factory import resource_client_factory

    try:
        resource_client = resource_client_factory(cli_ctx).resource_groups
        return resource_client.get(resource_group_name), resource_client.config.subscription_id
    except Exception as ex:  # pylint: disable=broad-except
        error = getattr(ex, 'Azure Error', ex)
        if error != 'ResourceGroupNotFound':
            return None, resource_client.config.subscription_id
        raise


def create_resource_group_name(cli_ctx, resource_group_name, location, tags=None):
    from ._client_factory import resource_client_factory

    ResourceGroup = get_sdk(cli_ctx, ResourceType.MGMT_RESOURCE_RESOURCES,
                            'ResourceGroup', mod='models')
    resource_client = resource_client_factory(cli_ctx).resource_groups
    parameters = ResourceGroup(location=location, tags=tags)
    return resource_client.create_or_update(resource_group_name, parameters), resource_client.config.subscription_id


def set_appconfig_keys(cli_ctx, appconfig_conn_string, kvs):
    from azure.cli.command_modules.appconfig._azconfig.azconfig_client import AzconfigClient
    from azure.cli.command_modules.appconfig._azconfig.models import KeyValue

    azconfig_client = AzconfigClient(appconfig_conn_string)

    for kv in kvs:
        set_kv = KeyValue(key=kv['key'], value=kv['value'])
        azconfig_client.set_keyvalue(set_kv)


def create_resource_manager_sp(cmd):
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


def zip_deploy_app(cli_ctx, resource_group_name, name, zip_url, slot=None, app_instance=None, timeout=None):
    import requests
    import urllib3

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

    logger.warning('Starting zip deployment. This may take several minutes to complete...')
    res = requests.put(zipdeploy_url, headers=authorization,
                       json={'packageUri': zip_url}, verify=not should_disable_connection_verify())

    # check if there's an ongoing process
    if res.status_code == 409:
        raise CLIError('There may be an ongoing deployment or your app setting has WEBSITE_RUN_FROM_PACKAGE. '
                       'Please track your deployment in {} and ensure the WEBSITE_RUN_FROM_PACKAGE app setting '
                       'is removed.'.format(deployment_status_url))

    # check the status of async deployment
    response = _check_zip_deployment_status(cli_ctx, resource_group_name, name, deployment_status_url,
                                            authorization, slot=slot, app_instance=app_instance, timeout=timeout)

    return response


def deploy_arm_template_at_resource_group(cmd, resource_group_name=None, template_file=None,
                                          template_uri=None, parameters=None, no_wait=False):
    from azure.cli.core.util import random_string
    from azure.cli.command_modules.resource.custom import (  # pylint: disable=unused-import
        deploy_arm_template_at_resource_group as _deploy_arm_template, get_deployment_at_resource_group)

    deployment_name = random_string(length=14, force_lower=True)

    deployment_poller = _deploy_arm_template(cmd, resource_group_name, template_file,
                                             template_uri, parameters, deployment_name,
                                             mode='incremental', no_wait=no_wait)

    deployment = LongRunningOperation(cmd.cli_ctx)(deployment_poller)

    properties = getattr(deployment, 'properties', None)
    # provisioning_state = getattr(properties, 'provisioning_state', None)
    outputs = getattr(properties, 'outputs', None)

    return outputs


def open_url_in_browser(url):
    # if we are not in cloud shell and can launch a browser, launch it with the issue draft
    if can_launch_browser() and not in_cloud_console():
        open_page_in_browser(url)
    else:
        print("There isn't an available browser finish the setup. Please copy and paste the url"
              " below in a browser to complete the configuration.\n\n{}\n\n".format(url))


def get_index(index_url):
    import requests
    for try_number in range(TRIES):
        try:
            response = requests.get(index_url, verify=(not should_disable_connection_verify()))
            if response.status_code == 200:
                return response.json()
            msg = ERR_TMPL_NON_200.format(response.status_code, index_url)
            raise CLIError(msg)
        except (requests.exceptions.ConnectionError, requests.exceptions.HTTPError) as err:
            msg = ERR_TMPL_NO_NETWORK.format(str(err))
            raise CLIError(msg)
        except ValueError as err:
            # Indicates that url is not redirecting properly to intended index url, we stop retrying after TRIES calls
            if try_number == TRIES - 1:
                msg = ERR_TMPL_BAD_JSON.format(str(err))
                raise CLIError(msg)
            import time
            time.sleep(0.5)
            continue


def get_index_providers(index_url):
    index = get_index(index_url=index_url)
    providers = index.get('providers')
    if providers is None:
        logger.warning(ERR_UNABLE_TO_GET_PROVIDERS)
    return providers


def get_index_teamcloud(index_url):
    index = get_index(index_url=index_url)
    teamcloud = index.get('teamcloud')
    if teamcloud is None:
        logger.warning(ERR_UNABLE_TO_GET_TEAMCLOUD)
    return teamcloud


def _check_zip_deployment_status(cli_ctx, resource_group_name, name, deployment_status_url,
                                 authorization, slot=None, app_instance=None, timeout=None):
    import json
    import requests
    from time import sleep

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

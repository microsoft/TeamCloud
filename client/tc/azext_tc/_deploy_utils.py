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
ERR_UNABLE_TO_GET_WEBAPP = 'Unable to get web app from index. Improper index format.'

TRIES = 3

logger = get_logger(__name__)


def get_github_release(cli_ctx, repo, org='microsoft', version=None, prerelease=False):
    import requests

    if version and prerelease:
        raise CLIError(
            'usage error: can only use one of --version/-v | --pre')

    url = 'https://api.github.com/repos/{}/{}/releases'.format(org, repo)

    if prerelease:
        version_res = requests.get(url, verify=not should_disable_connection_verify())
        version_json = version_res.json()

        version_prerelease = next((v for v in version_json if v['prerelease']), None)
        if not version_prerelease:
            raise CLIError('--pre no prerelease versions found for {}/{}'.format(org, repo))

        return version_prerelease

    url += ('/tags/{}'.format(version) if version else '/latest')

    version_res = requests.get(url, verify=not should_disable_connection_verify())

    if version_res.status_code == 404:
        raise CLIError(
            'No release version exists for {}/{}. '
            'Specify a specific prerelease version with --version '
            'or use latest prerelease with --pre'.format(org, repo))

    return version_res.json()


def get_github_latest_release_version(cli_ctx, repo, org='microsoft', prerelease=False):
    version_json = get_github_release(cli_ctx, repo, org, prerelease=prerelease)
    return version_json['tag_name']


def github_release_version_exists(cli_ctx, version, repo, org='microsoft'):
    import requests

    version_url = 'https://api.github.com/repos/{}/{}/releases/tags/{}'.format(org, repo, version)
    version_res = requests.get(version_url, verify=not should_disable_connection_verify())
    return version_res.status_code < 400


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
    parameters = ResourceGroup(location=location.lower(), tags=tags)
    return resource_client.create_or_update(resource_group_name, parameters), resource_client.config.subscription_id


def set_appconfig_keys(cmd, appconfig_conn_string, kvs):
    from azure.cli.command_modules.appconfig.keyvalue import set_key

    for kv in kvs:
        set_key(cmd, key=kv['key'], value=kv['value'], yes=True,
                connection_string=appconfig_conn_string)


def create_resource_manager_sp(cmd, app_name):
    from azure.cli.command_modules.role.custom import create_service_principal_for_rbac, add_permission, admin_consent

    sp = create_service_principal_for_rbac(cmd, name='http://TeamCloud.' + (app_name or 'ResourceManager'),
                                           years=10, role='Owner')
    # Azure Active Directory Graph permissions
    add_permission(cmd, identifier=sp['appId'], api='00000002-0000-0000-c000-000000000000',
                   api_permissions=['5778995a-e1bf-45b8-affa-663a9f3f4d04=Role',  # Directory.Read.All
                                    '824c81eb-e3f8-4ee6-8f6d-de7f50d565b7=Role'])  # Application.ReadWrite.OwnedBy
    # Microsoft Graph permissions
    add_permission(cmd, identifier=sp['appId'], api='00000003-0000-0000-c000-000000000000',
                   api_permissions=['7ab1d382-f21e-4acd-a863-ba3e13f7da61=Role',  # Directory.Read.All
                                    '18a4783c-866b-4cc7-a460-3d5e5662c884=Role'])  # Application.ReadWrite.OwnedBy

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
    from azure.cli.core.util import random_string, sdk_no_wait
    from azure.cli.command_modules.resource.custom import _prepare_deployment_properties_unmodified
    from ._client_factory import resource_client_factory

    properties = _prepare_deployment_properties_unmodified(cmd, template_file=template_file,
                                                           template_uri=template_uri, parameters=parameters,
                                                           mode='Incremental')

    client = resource_client_factory(cmd.cli_ctx).deployments

    for try_number in range(TRIES):
        try:
            deployment_name = random_string(length=14, force_lower=True) + str(try_number)

            if cmd.supported_api_version(min_api='2019-10-01', resource_type=ResourceType.MGMT_RESOURCE_RESOURCES):
                Deployment = cmd.get_models(
                    'Deployment', resource_type=ResourceType.MGMT_RESOURCE_RESOURCES)

                deployment = Deployment(properties=properties)
                deploy_poll = sdk_no_wait(no_wait, client.create_or_update, resource_group_name,
                                          deployment_name, deployment)
            else:
                deploy_poll = sdk_no_wait(no_wait, client.create_or_update, resource_group_name,
                                          deployment_name, properties)

            result = LongRunningOperation(cmd.cli_ctx, start_msg='Deploying ARM template',
                                          finish_msg='Finished deploying ARM template')(deploy_poll)

            props = getattr(result, 'properties', None)
            return getattr(props, 'outputs', None)
        except CLIError as err:
            if try_number == TRIES - 1:
                raise err
            try:
                response = getattr(err, 'response', None)
                import json
                message = json.loads(response.text)['error']['details'][0]['message']
                if '(ServiceUnavailable)' not in message:
                    raise err
            except:
                raise err
            import time
            time.sleep(5)
            continue


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


def get_index_providers_core(cli_ctx, version=None, prerelease=False, index_url=None, warn=False):
    if index_url is None:
        version = version or get_github_latest_release_version(
            cli_ctx, 'TeamCloud-Providers', prerelease=prerelease)
        index_url = 'https://github.com/microsoft/TeamCloud-Providers/releases/download/{}/index.json'.format(
            version)
    index = get_index(index_url=index_url)
    providers = index.get('providers')
    if warn and providers is None:
        logger.warning(ERR_UNABLE_TO_GET_PROVIDERS)
    return version, providers


def get_index_providers(cli_ctx, provider, version=None, prerelease=False, index_url=None):
    version, providers = get_index_providers_core(cli_ctx, version, prerelease, index_url)
    index = providers.get(provider)
    if not index:
        raise CLIError("--name/-n no provider found in index with id '{}'".format(provider))
    zip_url, deploy_url, provider_name, provider_type = index.get(
        'zipUrl'), index.get('deployUrl'), index.get('name'), index.get('type')

    if not zip_url:
        raise CLIError("No zipUrl found in index for provider with id '{}'".format(provider))
    if not deploy_url:
        raise CLIError("No deployUrl found in index for provider with id '{}'".format(provider))
    if not provider_name:
        raise CLIError("No name found in index for provider with id '{}'".format(provider))
    if not provider_type:
        raise CLIError("No type found in index for provider with id '{}'".format(provider))

    return version, zip_url, deploy_url, provider_name, provider_type


def _get_index_teamcloud_core(cli_ctx, version=None, prerelease=False, index_url=None):
    if index_url is None:
        version = version or get_github_latest_release_version(
            cli_ctx, 'TeamCloud', prerelease=prerelease)
        index_url = 'https://github.com/microsoft/TeamCloud/releases/download/{}/index.json'.format(
            version)
    index = get_index(index_url=index_url)
    teamcloud = index.get('teamcloud')
    if teamcloud is None:
        logger.warning(ERR_UNABLE_TO_GET_TEAMCLOUD)
    webapp = index.get('webapp')
    if webapp is None:
        logger.warning(ERR_UNABLE_TO_GET_WEBAPP)
    return version, teamcloud, webapp


def get_index_teamcloud(cli_ctx, version=None, prerelease=False, index_url=None):
    version, teamcloud, _ = _get_index_teamcloud_core(cli_ctx, version, prerelease, index_url)
    deploy_url, api_zip_url, orchestrator_zip_url = teamcloud.get('deployUrl'), teamcloud.get(
        'apiZipUrl'), teamcloud.get('orchestratorZipUrl')
    if not deploy_url:
        raise CLIError('No deployUrl found in index')
    if not api_zip_url:
        raise CLIError('No apiZipUrl found in index')
    if not orchestrator_zip_url:
        raise CLIError('No orchestratorZipUrl found in index')
    return version, deploy_url, api_zip_url, orchestrator_zip_url


def get_index_webapp(cli_ctx, version=None, prerelease=False, index_url=None):
    version, _, webapp = _get_index_teamcloud_core(cli_ctx, version, prerelease, index_url)
    deploy_url, zip_url = webapp.get('deployUrl'), webapp.get('zipUrl')
    if not deploy_url:
        raise CLIError('No deployUrl found in index')
    if not zip_url:
        raise CLIError('No zipUrl found in index')
    return version, deploy_url, zip_url


def get_app_name(url):
    from re import match
    name = ''
    m = match(r'^https?://(?P<name>[a-zA-Z0-9-]+)\.azurewebsites\.net[/a-zA-Z0-9.\:]*$', url)
    try:
        name = m.group('name') if m is not None else None
    except IndexError:
        pass

    if name is None or '':
        raise CLIError('Unable to get function app name from url.')

    return name


def get_arm_output(outputs, key, raise_on_error=True):
    try:
        value = outputs[key]['value']
    except KeyError:
        if raise_on_error:
            raise CLIError(
                "A value for '{}' was not provided in the ARM template outputs".format(key))
        value = None

    return value


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

    # logger.warning('Configuring default logging for the app, if not already enabled...')

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

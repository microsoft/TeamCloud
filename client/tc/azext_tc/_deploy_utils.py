# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------
# pylint: disable=unused-argument, protected-access, too-many-lines
# pylint: disable=inconsistent-return-statements

import json
import requests

from knack.util import CLIError
from knack.log import get_logger
from azure.cli.core.commands import LongRunningOperation
from azure.cli.core.commands.client_factory import get_subscription_id
from azure.cli.core.profiles import ResourceType, get_sdk
from azure.cli.core.util import (can_launch_browser, open_page_in_browser, in_cloud_console,
                                 random_string, sdk_no_wait, should_disable_connection_verify)


from ._client_factory import (deployment_client_factory, resource_client_factory)


ERR_TMPL_INDEX = 'Unable to get provider index.\n'
ERR_TMPL_NON_200 = f'{ERR_TMPL_INDEX}Server returned status code {{}} for {{}}'
ERR_TMPL_NO_NETWORK = f'{ERR_TMPL_INDEX}Please ensure you have network connection. Error detail: {{}}'
ERR_TMPL_BAD_JSON = f'{ERR_TMPL_INDEX}Response body does not contain valid json. Error detail: {{}}'
ERR_UNABLE_TO_GET_TEAMCLOUD = 'Unable to get teamcloud from index. Improper index format.'

TRIES = 3

logger = get_logger(__name__)


def get_github_release(repo, org='microsoft', version=None, prerelease=False):
    if version and prerelease:
        raise CLIError(
            'usage error: can only use one of --version/-v | --pre')

    url = f'https://api.github.com/repos/{org}/{repo}/releases'

    if prerelease:
        version_res = requests.get(url, verify=not should_disable_connection_verify())
        version_json = version_res.json()

        version_prerelease = next((v for v in version_json if v['prerelease']), None)
        if not version_prerelease:
            raise CLIError(f'--pre no prerelease versions found for {org}/{repo}')

        return version_prerelease

    url += (f'/tags/{version}' if version else '/latest')

    version_res = requests.get(url, verify=not should_disable_connection_verify())

    if version_res.status_code == 404:
        raise CLIError(
            f'No release version exists for {org}/{repo}. '
            'Specify a specific prerelease version with --version '
            'or use latest prerelease with --pre')

    return version_res.json()


def get_github_latest_release_version(repo, org='microsoft', prerelease=False):
    version_json = get_github_release(repo, org, prerelease=prerelease)
    return version_json['tag_name']


def github_release_version_exists(version, repo, org='microsoft'):
    version_url = f'https://api.github.com/repos/{org}/{repo}/releases/tags/{version}'
    version_res = requests.get(version_url, verify=not should_disable_connection_verify())
    return version_res.status_code < 400


def get_index(index_url):
    for try_number in range(TRIES):
        try:
            response = requests.get(index_url, verify=(not should_disable_connection_verify()))
            if response.status_code == 200:
                return response.json()
            msg = ERR_TMPL_NON_200.format(response.status_code, index_url)
            raise CLIError(msg)
        except (requests.exceptions.ConnectionError, requests.exceptions.HTTPError) as err:
            msg = ERR_TMPL_NO_NETWORK.format(str(err))
            raise CLIError(msg) from err
        except ValueError as err:
            # Indicates that url is not redirecting properly to intended index url, we stop retrying after TRIES calls
            if try_number == TRIES - 1:
                msg = ERR_TMPL_BAD_JSON.format(str(err))
                raise CLIError(msg) from err
            import time
            time.sleep(0.5)
            continue


def get_local_index(index_file):
    from azure.cli.core.util import get_file_json
    return get_file_json(index_file, preserve_order=True)


def get_teamcloud_index(version=None, prerelease=False, index_file=None, index_url=None):
    if index_file is not None:
        index = get_local_index(index_file=index_file)
    else:
        if index_url is None:
            version = version or get_github_latest_release_version('TeamCloud', prerelease=prerelease)
            index_url = f'https://github.com/microsoft/TeamCloud/releases/download/{version}/index.json'
        index = get_index(index_url=index_url)

    teamcloud = index.get('teamcloud')

    if teamcloud is None:
        logger.warning(ERR_UNABLE_TO_GET_TEAMCLOUD)

    deploy_url = teamcloud.get('deployUrl')

    if not deploy_url:
        raise CLIError('No deployUrl found in index')
    return version, deploy_url


def get_resource_group_by_name(cli_ctx, resource_group_name):
    subscription_id = get_subscription_id(cli_ctx)
    try:
        resource_client = resource_client_factory(cli_ctx).resource_groups
        return resource_client.get(resource_group_name), subscription_id
    except Exception as ex:  # pylint: disable=broad-except
        error = getattr(ex, 'Azure Error', ex)
        if error != 'ResourceGroupNotFound':
            return None, subscription_id
        raise


def create_resource_group_name(cli_ctx, resource_group_name, location, tags=None):
    subscription_id = get_subscription_id(cli_ctx)
    ResourceGroup = get_sdk(cli_ctx, ResourceType.MGMT_RESOURCE_RESOURCES,
                            'ResourceGroup', mod='models')
    resource_client = resource_client_factory(cli_ctx).resource_groups
    parameters = ResourceGroup(location=location.lower(), tags=tags)
    return resource_client.create_or_update(resource_group_name, parameters), subscription_id


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


def deploy_arm_template_at_resource_group(cmd, resource_group_name=None, template_file=None,
                                          template_uri=None, parameters=None, no_wait=False):

    from azure.cli.command_modules.resource.custom import _prepare_deployment_properties_unmodified

    deployment_properties = _prepare_deployment_properties_unmodified(cmd, 'resourceGroup', template_file=template_file,
                                                                      template_uri=template_uri, parameters=parameters,
                                                                      mode='Incremental')

    deployment_client = deployment_client_factory(cmd.cli_ctx, plug_pipeline=(template_uri is None))

    for try_number in range(TRIES):
        try:
            deployment_name = random_string(length=14, force_lower=True) + str(try_number)

            Deployment = cmd.get_models('Deployment', resource_type=ResourceType.MGMT_RESOURCE_RESOURCES)
            deployment = Deployment(properties=deployment_properties)

            deploy_poll = sdk_no_wait(no_wait, deployment_client.begin_create_or_update, resource_group_name,
                                      deployment_name, deployment)

            result = LongRunningOperation(cmd.cli_ctx, start_msg='Deploying ARM template',
                                          finish_msg='Finished deploying ARM template')(deploy_poll)

            props = getattr(result, 'properties', None)
            return getattr(props, 'outputs', None)
        except CLIError as err:
            if try_number == TRIES - 1:
                raise err
            try:
                response = getattr(err, 'response', None)
                message = json.loads(response.text)['error']['details'][0]['message']
                if '(ServiceUnavailable)' not in message:
                    raise err
            except:
                raise err from err
            import time
            time.sleep(5)
            continue


def open_url_in_browser(url):
    # if we are not in cloud shell and can launch a browser, launch it with the issue draft
    if can_launch_browser() and not in_cloud_console():
        open_page_in_browser(url)
    else:
        print("There isn't an available browser finish the setup. Please copy and paste the url"
              f" below in a browser to complete the configuration.\n\n{url}\n\n")


def get_arm_output(outputs, key, raise_on_error=True):
    try:
        value = outputs[key]['value']
    except KeyError as e:
        if raise_on_error:
            raise CLIError(
                f"A value for '{key}' was not provided in the ARM template outputs") from e
        value = None

    return value

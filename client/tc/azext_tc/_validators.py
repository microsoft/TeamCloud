# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

from re import match
from uuid import UUID
from azure.cli.core.util import CLIError
from knack.log import get_logger
from .vendored_sdks.teamcloud.models import ErrorResult

logger = get_logger(__name__)

# pylint: disable=unused-argument, protected-access


def tc_deploy_validator(cmd, namespace):
    if namespace.principal_name is not None:
        if namespace.principal_password is None:
            raise CLIError(
                '--principal-password must be have a value if --principal-name is specified')
    if namespace.principal_password is not None:
        if namespace.principal_name is None:
            raise CLIError(
                '--principal-name must be have a value if --principal-password is specified')

    if namespace.version and namespace.prerelease:
        raise CLIError('Invalid argument usage: --version | --pre')

    if namespace.version:
        namespace.version = namespace.version.lower()
        if namespace.version[:1].isdigit():
            namespace.version = 'v' + namespace.version
        if not _is_valid_version(namespace.version):
            raise CLIError(
                '--version should be in format v0.0.0 do not include -pre suffix')

        import requests
        from azure.cli.core.util import should_disable_connection_verify

        version_url = 'https://api.github.com/repos/microsoft/TeamCloud/releases/tags/' + namespace.version
        version_res = requests.get(version_url, verify=not should_disable_connection_verify())
        if version_res.status_code == 404:
            raise CLIError('--version {} does not exist'.format(namespace.version))

    if namespace.name is not None:
        name_clean = ''
        for n in namespace.name.lower():
            if n.isalpha() or n.isdigit() or n == '-':
                name_clean += n

        namespace.name = name_clean

        if namespace.skip_name_validation:
            logger.warning('IMPORTANT: --skip-name-validation prevented unique name validation.')
        else:
            from ._client_factory import web_client_factory

            web_client = web_client_factory(cmd.cli_ctx)
            availability = web_client.check_name_availability(namespace.name, 'Site')
            if not availability.name_available:
                raise CLIError(
                    '--name {}'.format(availability.message))


def project_name_validator(cmd, namespace):
    if namespace.name is not None:
        if _is_valid_uuid(namespace.name):
            return
        if not _is_valid_project_name(namespace.name):
            raise CLIError(
                '--project should be a valid uuid or a project name string with length [4,31]')


def project_name_or_id_validator(cmd, namespace):
    if namespace.project:
        if _is_valid_uuid(namespace.project):
            return
        if not _is_valid_project_name(namespace.project):
            raise CLIError(
                '--project should be a valid uuid or a project name string with length [4,31]')

        from ._client_factory import teamcloud_client_factory

        client = teamcloud_client_factory(cmd.cli_ctx)
        client._client.config.base_url = namespace.base_url
        result = client.get_project_by_name_or_id(namespace.project)

        if isinstance(result, ErrorResult):
            raise CLIError(
                '--project no project found matching provided project name')
        try:
            namespace.project = result.data.id
        except AttributeError:
            pass


def user_name_or_id_validator(cmd, namespace):
    if namespace.user:
        if _is_valid_uuid(namespace.user):
            return
        if not _has_basic_email_format(namespace.user):
            raise CLIError(
                '--user should be a valid uuid or a user name in eamil format')


def project_type_id_validator(cmd, namespace):
    if namespace.project_type:
        if not _is_valid_project_type_id(namespace.project_type):
            raise CLIError(
                '--project-type should start with a lowercase and contain only lowercase, numbers, '
                'and periods [.] with length [5,254]')


def project_type_id_validator_name(cmd, namespace):
    if namespace.project_type:
        if not _is_valid_project_type_id(namespace.project_type):
            raise CLIError(
                '--name should start with a lowercase and contain only lowercase, numbers, '
                'and periods [.] with length [5,254]')


def provider_id_validator(cmd, namespace):
    if namespace.provider:
        if not _is_valid_provider_id(namespace.provider):
            raise CLIError(
                '--name should start with a lowercase and contain only lowercase, numbers, '
                'and periods [.] with length [5,254]')


def subscriptions_list_validator(cmd, namespace):
    if namespace.subscriptions:
        if not all(_is_valid_uuid(x) for x in namespace.subscriptions):
            raise CLIError(
                '--subscriptions should be a space-separated list of valid uuids')


def provider_event_list_validator(cmd, namespace):
    if namespace.events:
        if not all(_is_valid_provider_id(x) for x in namespace.events):
            raise CLIError(
                '--events should be a space-separated list of valid provider ids, '
                'provider ids should start with a lowercase and contain only lowercase, numbers, '
                'and periods [.] with length [5,254]')


def provider_depends_on_validator(cmd, namespace):
    if namespace.depends_on:
        if not all(_is_valid_provider_id(x) for x in namespace.depends_on):
            raise CLIError(
                '--depends-on should be a space-separated list of valid provider ids, '
                'provider ids should start with a lowercase and contain only lowercase, numbers, '
                'and periods [.] with length [5,254]')


def tc_resource_name_validator(cmd, namespace):
    if namespace.name:
        if match(r'^[^\\/\?#]{2,254}$', namespace.name) is None:
            raise CLIError(
                r"--name should have length [2,254] and not include characters [ '\\', '/', '?', '#' ]")


def tracking_id_validator(cmd, namespace):
    if namespace.tracking_id:
        if not _is_valid_uuid(namespace.tracking_id):
            raise CLIError(
                '--tracking-id should be a valid uuid')


def url_validator(cmd, namespace):
    if namespace.url:
        if not _is_valid_url(namespace.url):
            raise CLIError(
                '--url should be a valid url')


def base_url_validator(cmd, namespace):
    if namespace.base_url:
        if not _is_valid_url(namespace.base_url):
            raise CLIError(
                '--base-url should be a valid url')


def auth_code_validator(cmd, namespace):
    if namespace.auth_code:
        if not _is_valid_functions_auth_code(namespace.auth_code):
            raise CLIError(
                '--auth-code should contain only base-64 digits [A-Za-z0-9/] '
                '(excluding the plus sign (+)), ending in = or ==')


def source_version_validator(cmd, namespace):
    if namespace.version:
        if namespace.prerelease:
            raise CLIError('--version | --pre')
        namespace.version = namespace.version.lower()
        if namespace.version[:1].isdigit():
            namespace.version = 'v' + namespace.version
        if not _is_valid_version(namespace.version):
            raise CLIError(
                '--version should be in format v0.0.0 do not include -pre suffix')


def teamcloud_source_version_validator(cmd, namespace):
    if namespace.version:
        if namespace.prerelease:
            raise CLIError('Invalid argument usage: --version | --pre')
        namespace.version = namespace.version.lower()
        if namespace.version[:1].isdigit():
            namespace.version = 'v' + namespace.version
        if not _is_valid_version(namespace.version):
            raise CLIError(
                '--version should be in format v0.0.0 do not include -pre suffix')

        import requests
        from azure.cli.core.util import should_disable_connection_verify

        version_url = 'https://api.github.com/repos/microsoft/TeamCloud/releases/tags/' + namespace.version
        version_res = requests.get(version_url, verify=not should_disable_connection_verify())
        if version_res.status_code == 404:
            raise CLIError('--version {} does not exist'.format(namespace.version))


def providers_source_version_validator(cmd, namespace):
    if namespace.version:
        if namespace.prerelease:
            raise CLIError('Invalid argument usage: --version | --pre')
        namespace.version = namespace.version.lower()
        if namespace.version[:1].isdigit():
            namespace.version = 'v' + namespace.version
        if not _is_valid_version(namespace.version):
            raise CLIError(
                '--version should be in format v0.0.0 do not include -pre suffix')

        import requests
        from azure.cli.core.util import should_disable_connection_verify

        version_url = 'https://api.github.com/repos/microsoft/TeamCloud-Providers/releases/tags/' + namespace.version
        version_res = requests.get(version_url, verify=not should_disable_connection_verify())
        if version_res.status_code == 404:
            raise CLIError('--version {} does not exist'.format(namespace.version))


def properties_validator(cmd, namespace):
    if isinstance(namespace.properties, list):
        properties_dict = {}
        for item in namespace.properties:
            properties_dict.update(property_validator(item))
        namespace.properties = properties_dict


def property_validator(string):
    result = {}
    if string:
        comps = string.split('=', 1)
        result = {comps[0]: comps[1]} if len(comps) > 1 else {string: ''}
    return result


def _is_valid_project_name(name):
    return name is not None and 4 < len(name) < 31


def _is_valid_project_type_id(project_type_id):
    return 5 <= len(project_type_id) <= 255 and match(
        r'^(?:[a-z][a-z0-9]+(?:\.?[a-z0-9]+)+)$', project_type_id) is not None


def _is_valid_provider_id(provider_id):
    return 5 <= len(provider_id) <= 255 and match(
        r'^(?:[a-z][a-z0-9]+(?:\.?[a-z0-9]+)+)$', provider_id) is not None


def _is_valid_resource_id(resource_id):
    return match(r'^[^\\/\?#]{2,255}$', resource_id) is not None


def _has_basic_email_format(email):
    """Basic email check to ensure it has exactly one @ sign,
    and at least one . in the part after the @ sign
    """
    return match(r'^[^@]+@[^@]+\.[^@]+$', email) is not None


def _is_valid_url(url):
    return match(
        r'^http[s]?://(?:[a-zA-Z]|[0-9]|[$-_@.&+]|[!*\(\), ]|(?:%[0-9a-fA-F][0-9a-fA-F]))+$', url) is not None


def _is_valid_functions_auth_code(auth_code):
    """Validates a string only contains base-64 digits,
    minus the plus sign (+) [A-Za-z0-9/], ends in = or ==
    https://github.com/Azure/azure-functions-host/blob/dev/src/WebJobs.Script.WebHost/Security/KeyManagement/SecretManager.cs#L592-L603
    """
    return match(r'^([A-Za-z0-9/]{4})*([A-Za-z0-9/]{3}=|[A-Za-z0-9/]{2}==)?$', auth_code) is not None


def _is_valid_uuid(uuid_to_test, version=4):
    try:
        uuid_obj = UUID(uuid_to_test, version=version)
    except ValueError:
        return False

    return str(uuid_obj) == uuid_to_test


def _is_valid_version(version):
    return match(r'^v[0-9]+\.[0-9]+\.[0-9]+$', version) is not None

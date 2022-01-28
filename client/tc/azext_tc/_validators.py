# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------
# pylint: disable=unused-argument, protected-access

from re import match
from uuid import UUID
from knack.log import get_logger
from azure.cli.core.util import CLIError
from azure.cli.core.commands.validators import validate_tags

from ._client_factory import web_client_factory, teamcloud_client_factory
from ._deploy_utils import github_release_version_exists

from .vendored_sdks.teamcloud.models import ErrorResult


logger = get_logger(__name__)


def _ensure_base_url(client, base_url):
    client._client._base_url = base_url


def tc_deploy_validator(cmd, ns):
    if ns.principal_name is not None:
        if ns.principal_password is None:
            raise CLIError(
                'usage error: --principal-password must be have a value if --principal-name is specified')
    if ns.principal_password is not None:
        if ns.principal_name is None:
            raise CLIError(
                'usage error: --principal-name must be have a value if --principal-password is specified')

    if sum(1 for ct in [ns.version, ns.prerelease, ns.index_url, ns.index_file] if ct) > 1:
        raise CLIError(
            'usage error: can only use one of --index-url | --index-file | --version/-v | --pre')

    if ns.version:
        ns.version = ns.version.lower()
        if ns.version[:1].isdigit():
            ns.version = 'v' + ns.version
        if not _is_valid_version(ns.version):
            raise CLIError(
                '--version/-v should be in format v0.0.0 do not include -pre suffix')

        if not github_release_version_exists(ns.version, 'TeamCloud'):
            raise CLIError(f'--version/-v {ns.version} does not exist')

    if ns.tags:
        validate_tags(ns)

    if ns.index_url:
        if not _is_valid_url(ns.index_url):
            raise CLIError(
                '--index-url should be a valid url')

    if ns.name is not None:
        name_clean = ''
        for n in ns.name.lower():
            if n.isalpha() or n.isdigit() or n == '-':
                name_clean += n

        ns.name = name_clean

        if ns.skip_name_validation:
            logger.warning('IMPORTANT: --skip-name-validation prevented unique name validation.')
        else:
            web_client = web_client_factory(cmd.cli_ctx)
            availability = web_client.check_name_availability(ns.name, 'Site')
            if not availability.name_available:
                raise CLIError(f'--name/-n {availability.message}')
    if ns.client_id:
        if not _is_valid_uuid(ns.client_id):
            raise CLIError('--client-id/-c should be a valid uuid')


def org_name_validator(cmd, ns):
    if ns.name is not None:
        if _is_valid_uuid(ns.name):
            return
        if not _is_valid_org_name(ns.name):
            raise CLIError(
                '--name/-n should be a valid uuid or a org name string with length [2,31]')


def org_name_or_id_validator(cmd, ns):
    if ns.org:
        if _is_valid_uuid(ns.org):
            return
        if not _is_valid_org_name(ns.org):
            raise CLIError(
                '--org should be a valid uuid or a org name string with length [2,31]')

        client = teamcloud_client_factory(cmd.cli_ctx)
        _ensure_base_url(client, ns.base_url)
        result = client.get_organization(ns.org)

        if result is None or isinstance(result, ErrorResult):
            raise CLIError(
                '--org no org found matching provided org name or id')
        try:
            ns.org = result.data.id
        except AttributeError:
            pass


def project_name_validator(cmd, ns):
    if ns.name is not None:
        if _is_valid_uuid(ns.name):
            return
        if not _is_valid_project_name(ns.name):
            raise CLIError(
                '--name/-n should be a valid uuid or a project name string with length [4,31]')


def subscriptions_list_validator(cmd, ns):
    if ns.subscriptions:
        if not all(_is_valid_uuid(x) for x in ns.subscriptions):
            raise CLIError(
                '--subscriptions should be a space-separated list of valid uuids')


def client_id_validator(cmd, ns):
    if ns.client_id:
        if not _is_valid_uuid(ns.client_id):
            raise CLIError('--client-id/-c should be a valid uuid')


def url_validator(cmd, ns):
    if ns.url:
        if not _is_valid_url(ns.url):
            raise CLIError('--url should be a valid url')


def base_url_validator(cmd, ns):
    if ns.base_url:
        if not _is_valid_url(ns.base_url):
            raise CLIError('--base-url/-u should be a valid url')


def index_url_validator(cmd, ns):
    if ns.index_url:
        if ns.prerelease or ns.version:
            raise CLIError(
                'usage error: can only use one of --index-url | --version/-v | --pre')
        if not _is_valid_url(ns.index_url):
            raise CLIError('--index-url should be a valid url')


def repo_url_validator(cmd, ns):
    if ns.repo_url:
        if not _is_valid_url(ns.repo_url):
            raise CLIError('--repo-url/-r should be a valid url')


def auth_code_validator(cmd, ns):
    if ns.auth_code:
        if not _is_valid_functions_auth_code(ns.auth_code):
            raise CLIError(
                '--auth-code should contain only base-64 digits [A-Za-z0-9/] '
                '(excluding the plus sign (+)), ending in = or ==')


def input_json_validator(cmd, ns):
    ns.input_json = 'null' if ns.input_json is None else ns.input_json
    if not _is_valid_json(ns.input_json):
        raise CLIError('--input/-i must be a valid json string')


def source_version_validator(cmd, ns):
    if ns.version:
        if ns.prerelease or ns.index_url:
            raise CLIError(
                'usage error: can only use one of --index-url | --version/-v | --pre')
        ns.version = ns.version.lower()
        if ns.version[:1].isdigit():
            ns.version = 'v' + ns.version
        if not _is_valid_version(ns.version):
            raise CLIError(
                '--version/-v should be in format v0.0.0 do not include -pre suffix')


def teamcloud_source_version_validator(cmd, ns):
    if ns.version:
        if ns.prerelease or ns.index_url:
            raise CLIError(
                'usage error: can only use one of --index-url | --version/-v | --pre')
        ns.version = ns.version.lower()
        if ns.version[:1].isdigit():
            ns.version = 'v' + ns.version
        if not _is_valid_version(ns.version):
            raise CLIError(
                '--version/-v should be in format v0.0.0 do not include -pre suffix')

        if not github_release_version_exists(ns.version, 'TeamCloud'):
            raise CLIError(f'--version/-v {ns.version} does not exist')


def teamcloud_cli_source_version_validator(cmd, ns):
    if ns.version:
        if ns.prerelease:
            raise CLIError(
                'usage error: can only use one of --version/-v | --pre')
        ns.version = ns.version.lower()
        if ns.version[:1].isdigit():
            ns.version = 'v' + ns.version
        if not _is_valid_version(ns.version):
            raise CLIError(
                '--version/-v should be in format v0.0.0 do not include -pre suffix')

        if not github_release_version_exists(ns.version, 'TeamCloud'):
            raise CLIError(f'--version/-v {ns.version} does not exist')


def properties_validator(cmd, ns):
    if isinstance(ns.properties, list):
        properties_dict = {}
        for item in ns.properties:
            properties_dict.update(property_validator(item))
        ns.properties = properties_dict


def property_validator(string):
    result = {}
    if string:
        comps = string.split('=', 1)
        result = {comps[0]: comps[1]} if len(comps) > 1 else {string: ''}
    return result


def _is_valid_org_name(name):
    return name is not None and 1 < len(name) < 31


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


def _is_valid_json(value):
    import json
    try:
        json.loads(value)
        return True
    except ValueError:
        return False
    return False

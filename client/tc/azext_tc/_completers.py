# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

from azure.cli.core.decorators import Completer
from knack.log import get_logger
from ._client_factory import teamcloud_client_factory

logger = get_logger(__name__)


@Completer
def get_project_completion_list(cmd, prefix, namespace, **kwargs):  # pylint: disable=unused-argument
    client = teamcloud_client_factory(cmd.cli_ctx)
    client._client.config.base_url = namespace.base_url  # pylint: disable=protected-access
    result = client.get_projects()

    try:
        return [p.name for p in result.data]
    except AttributeError:
        return []


@Completer
def get_project_type_completion_list(cmd, prefix, namespace, **kwargs):  # pylint: disable=unused-argument
    client = teamcloud_client_factory(cmd.cli_ctx)
    client._client.config.base_url = namespace.base_url  # pylint: disable=protected-access
    result = client.get_project_types()

    try:
        return [p.id for p in result.data]
    except AttributeError:
        return []


@Completer
def get_provider_completion_list(cmd, prefix, namespace, **kwargs):  # pylint: disable=unused-argument
    client = teamcloud_client_factory(cmd.cli_ctx)
    client._client.config.base_url = namespace.base_url  # pylint: disable=protected-access
    result = client.get_providers()

    try:
        return [p.id for p in result.data]
    except AttributeError:
        return []


@Completer
def get_provider_index_completion_list(cmd, prefix, namespace, **kwargs):  # pylint: disable=unused-argument
    from ._deploy_utils import get_index_providers, get_github_latest_release

    if namespace.version or namespace.prerelease:
        if namespace.index_url:
            return []

    if namespace.index_url is None:
        version = namespace.version or get_github_latest_release(
            cmd.cli_ctx, 'TeamCloud-Providers', prerelease=namespace.prerelease)
        index_url = 'https://github.com/microsoft/TeamCloud-Providers/releases/download/{}/index.json'.format(
            version)

    index_providers = get_index_providers(index_url=index_url)

    if not index_providers:
        return []

    try:
        return [p for p in index_providers]
    except (AttributeError, ValueError):
        return []

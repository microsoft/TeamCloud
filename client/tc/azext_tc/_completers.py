# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

from azure.cli.core.decorators import Completer
from knack.log import get_logger
from ._client_factory import teamcloud_client_factory

logger = get_logger(__name__)


def _ensure_base_url(client, base_url):
    client._client._base_url = base_url  # pylint: disable=protected-access


@Completer
def get_project_completion_list(cmd, prefix, namespace, **kwargs):  # pylint: disable=unused-argument
    client = teamcloud_client_factory(cmd.cli_ctx)
    _ensure_base_url(client, namespace.base_url)
    result = client.get_projects()

    try:
        return [p.name for p in result.data]
    except AttributeError:
        return []


@Completer
def get_project_type_completion_list(cmd, prefix, namespace, **kwargs):  # pylint: disable=unused-argument
    client = teamcloud_client_factory(cmd.cli_ctx)
    _ensure_base_url(client, namespace.base_url)
    result = client.get_project_types()

    try:
        return [p.id for p in result.data]
    except AttributeError:
        return []


@Completer
def get_provider_completion_list(cmd, prefix, namespace, **kwargs):  # pylint: disable=unused-argument
    client = teamcloud_client_factory(cmd.cli_ctx)
    _ensure_base_url(client, namespace.base_url)
    result = client.get_providers()

    try:
        return [p.id for p in result.data]
    except AttributeError:
        return []


@Completer
def get_provider_completion_list_novirtual(cmd, prefix, namespace, **kwargs):  # pylint: disable=unused-argument
    client = teamcloud_client_factory(cmd.cli_ctx)
    _ensure_base_url(client, namespace.base_url)
    result = client.get_providers()

    try:
        return [p.id for p in result.data if p['type'] != 'Virtual']
    except AttributeError:
        return []


@Completer
def get_provider_index_completion_list(cmd, prefix, namespace, **kwargs):  # pylint: disable=unused-argument
    from ._deploy_utils import get_index_providers_core

    if namespace.version or namespace.prerelease:
        if namespace.index_url:
            return []

    _, index_providers = get_index_providers_core(
        cmd.cli_ctx, namespace.version, namespace.prerelease, namespace.index_url, False)

    if not index_providers:
        return []

    try:
        return [p for p in index_providers]
    except (AttributeError, ValueError):
        return []

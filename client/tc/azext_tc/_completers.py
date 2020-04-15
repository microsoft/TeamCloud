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

    # logger.warning('Completing...')

    client = teamcloud_client_factory(cmd.cli_ctx)
    client._client.config.base_url = namespace.base_url  # pylint: disable=protected-access
    result = client.get_projects()

    try:
        return [p.name for p in result.data]
    except AttributeError:
        return []


@Completer
def get_project_type_completion_list(cmd, prefix, namespace, **kwargs):  # pylint: disable=unused-argument

    # logger.warning('Completing...')

    client = teamcloud_client_factory(cmd.cli_ctx)
    client._client.config.base_url = namespace.base_url  # pylint: disable=protected-access
    result = client.get_project_types()

    try:
        return [p.id for p in result.data]
    except AttributeError:
        return []


@Completer
def get_provider_completion_list(cmd, prefix, namespace, **kwargs):  # pylint: disable=unused-argument

    # logger.warning('Completing...')

    client = teamcloud_client_factory(cmd.cli_ctx)
    client._client.config.base_url = namespace.base_url  # pylint: disable=protected-access
    result = client.get_providers()

    try:
        return [p.id for p in result.data]
    except AttributeError:
        return []

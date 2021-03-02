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
def get_org_completion_list(cmd, prefix, namespace, **kwargs):  # pylint: disable=unused-argument
    client = teamcloud_client_factory(cmd.cli_ctx)
    _ensure_base_url(client, namespace.base_url)

    result = client.get_organizations()

    try:
        return [p.displayName for p in result.data]
    except AttributeError:
        return []

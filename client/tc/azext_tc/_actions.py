# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------
# pylint: disable=protected-access, too-few-public-methods

import argparse

from knack.log import get_logger
from knack.util import CLIError

from .vendored_sdks.teamcloud.models import ProviderReference

from ._validators import _is_valid_provider_id

logger = get_logger(__name__)


class CreateProviderReference(argparse._AppendAction):

    def __call__(self, parser, namespace, values, option_string=None):

        if namespace.providers is None:
            namespace.providers = []

        try:
            provider_id = values.pop(0)

            if not _is_valid_provider_id(provider_id):
                raise CLIError(
                    'usage error: {} PROVIDER_ID [KEY=VALUE ...] PROVIDER_ID should start with '
                    'a lowercase and contain only lowercase, numbers, and periods [.] '
                    'with length [5,254]'.format(option_string or '--provider'))

            provider_depends_on = []
            provider_properties = {}

            for item in values:
                key, value = item.split('=', 1)
                if key.lower() == 'depends-on' or key.lower() == 'dependson' or key.lower() == 'depends_on':
                    if not _is_valid_provider_id(value):
                        raise CLIError(
                            'usage error: {} PROVIDER_ID [KEY=VALUE ...] DEPENDS_ON must be a valid '
                            'provider id, start with a lowercase and contain only lowercase, numbers, '
                            'and periods [.] with length [5,254]'.format(option_string or '--provider'))
                    provider_depends_on.append(value)
                else:
                    provider_properties[key] = value

            namespace.providers.append(ProviderReference(
                id=provider_id, properties=provider_properties, depends_on=provider_depends_on))

        except (IndexError, ValueError):
            raise CLIError('usage error: {} PROVIDER_ID [KEY=VALUE ...]'.format(
                option_string or '--provider'))

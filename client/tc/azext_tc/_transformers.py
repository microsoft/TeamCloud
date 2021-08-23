# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

from collections import OrderedDict
from knack.log import get_logger
from .vendored_sdks.teamcloud.models import (ErrorResult, StatusResult)

logger = get_logger(__name__)


def transform_output(result):

    if result is None:
        logger.warning('Consider raising exception')

    if isinstance(result, ErrorResult):
        return transform_error(result)

    if isinstance(result, StatusResult):
        return transform_status(result)

    # assume DataResult
    try:
        return result.data
    except AttributeError:
        return result


def transform_error(result):
    logger.error('Error: %s', result.status)
    return result


def transform_status(result):
    # If the StatusResult returns a 302 or 201 code msrest
    # automatically follows the location header sending a
    # GET request to retrieve the new object and stores the
    # GET response (DataResult) in additional_properties
    try:
        return result.additional_properties['data']
    except (AttributeError, KeyError):
        return result

# ----------------
# Table Output
# ----------------


def transform_org_table_output(result):
    if not isinstance(result, list):
        result = [result]

    resultList = []

    for item in result:
        resultList.append(OrderedDict([
            ('Name', item['displayName']),
            ('Slug', item['slug']),
            ('ID', item['id']),
            ('Location', item['location']),
            ('State', item['resourceState']),
            ('Subscription', item['subscriptionId']),
            ('Tags', str(item['tags'])),
        ]))

    return resultList


def transform_scope_table_output(result):
    if not isinstance(result, list):
        result = [result]

    resultList = []

    for item in result:
        resultList.append(OrderedDict([
            ('Name', item['displayName']),
            ('Slug', item['slug']),
            ('Type', item['type']),
            ('ID', item['id']),
            ('Authorized', item['authorized']),
            ('Component Types', ','.join(item['componentTypes'])),
        ]))

    return resultList


def transform_template_table_output(result):
    if not isinstance(result, list):
        result = [result]

    resultList = []

    for item in result:
        repo = item['repository']
        # components = item['components']
        resultList.append(OrderedDict([
            ('Name', item['displayName']),
            ('Slug', item['slug']),
            ('ID', item['id']),
            ('Default', item['isDefault']),
            ('Repository', '' if repo is None or repo['url'] is None else repo['url']),
            ('Version', '' if repo is None or repo['version'] is None else repo['version'])
            # ('Components', '' if components is None else '\n'.join(components))
        ]))

    return resultList


def transform_tag_table_output(result):
    if not isinstance(result, dict):
        result = {}

    resultList = []

    for k, v in result.items():
        resultList.append(OrderedDict([
            ('Key', k),
            ('Value', v)
        ]))

    return resultList

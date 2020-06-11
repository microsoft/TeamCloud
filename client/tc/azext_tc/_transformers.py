# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

from collections import OrderedDict
from knack.log import get_logger
from .vendored_sdks.teamcloud.models import (ErrorResult, StatusResult)

logger = get_logger(__name__)


def transform_output(result):

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


def transform_user_table_output(result):
    if not isinstance(result, list):
        result = [result]

    resultList = []

    for item in result:
        pm = item['projectMemberships']
        resultList.append(OrderedDict([
            ('User ID', item['id']),
            ('Type', item['userType']),
            ('Role', item['role']),
            ('Project Memberships', '' if pm is None else '\n'.join(
                ["{} : {}".format(p['role'], p['projectId']) for p in pm])),
            ('Properties', str(item['properties']))
        ]))

    return resultList


def transform_project_table_output(result):
    if not isinstance(result, list):
        result = [result]

    resultList = []

    for item in result:
        rg = item['resourceGroup']
        resultList.append(OrderedDict([
            ('Project ID', item['id']),
            ('Name', item['name']),
            ('Type', item['type']['id']),
            ('Resource Group', '' if rg is None else rg['name']),
            ('Subscription', '' if rg is None else rg['subscriptionId']),
            ('Region', '' if rg is None else rg['region']),
            ('Tags', str(item['tags'])),
            ('Properties', str(item['properties']))
        ]))

    return resultList


def transform_project_type_table_output(result):
    if not isinstance(result, list):
        result = [result]

    resultList = []

    for item in result:
        resultList.append(OrderedDict([
            ('Project Type ID', item['id']),
            ('Default', item['default']),
            ('Region', item['region']),
            ('Subscriptions', '\n'.join(item['subscriptions'])),
            ('Subscription Capacity', item['subscriptionCapacity']),
            ('Resource Group Prefix', item['resourceGroupNamePrefix']),
            ('Providers', '\n'.join([p['id'] for p in item['providers']])),
            ('Tags', str(item['tags'])),
            ('Properties', str(item['properties']))
        ]))

    return resultList


def transform_provider_table_output(result):
    if not isinstance(result, list):
        result = [result]

    resultList = []

    for item in result:
        resultList.append(OrderedDict([
            ('Provider ID', item['id']),
            ('Url', item['url']),
            ('Code', '************'),
            ('Events', '\n'.join(item['events'])),
            ('Properties', str(item['properties'])),
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

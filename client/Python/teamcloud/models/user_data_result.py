# coding=utf-8
# --------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for
# license information.
#
# Code generated by Microsoft (R) AutoRest Code Generator.
# Changes may cause incorrect behavior and will be lost if the code is
# regenerated.
# --------------------------------------------------------------------------

from msrest.serialization import Model


class UserDataResult(Model):
    """UserDataResult.

    :param code:
    :type code: int
    :param status:
    :type status: str
    :param data:
    :type data: ~teamcloud.models.User
    :param location:
    :type location: str
    """

    _attribute_map = {
        'code': {'key': 'code', 'type': 'int'},
        'status': {'key': 'status', 'type': 'str'},
        'data': {'key': 'data', 'type': 'User'},
        'location': {'key': 'location', 'type': 'str'},
    }

    def __init__(self, **kwargs):
        super(UserDataResult, self).__init__(**kwargs)
        self.code = kwargs.get('code', None)
        self.status = kwargs.get('status', None)
        self.data = kwargs.get('data', None)
        self.location = kwargs.get('location', None)

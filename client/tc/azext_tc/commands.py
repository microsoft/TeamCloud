# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

from ._client_factory import teamcloud_client_factory
from ._transformers import (transform_output, transform_user_table_output, transform_project_table_output,
                            transform_project_type_table_output, transform_provider_table_output,
                            transform_tag_table_output)
from ._validators import tc_deploy_validator


def load_command_table(self, _):

    # TeamCloud

    with self.command_group('tc', is_preview=True):
        pass

    with self.command_group('tc', client_factory=teamcloud_client_factory) as g:
        g.custom_command('deploy', 'teamcloud_deploy', validator=tc_deploy_validator)
        g.custom_command('upgrade', 'teamcloud_upgrade')
        g.custom_command('status', 'status_get', transform=transform_output)

    # TeamCloud Users

    with self.command_group('tc user', client_factory=teamcloud_client_factory) as g:
        g.custom_command('create', 'teamcloud_user_create', transform=transform_output,
                         supports_no_wait=True)
        g.custom_command('delete', 'teamcloud_user_delete', transform=transform_output,
                         supports_no_wait=True, confirmation='Are you sure you want to delete this user?')
        g.custom_command('list', 'teamcloud_user_list', transform=transform_output,
                         table_transformer=transform_user_table_output)
        g.custom_show_command('show', 'teamcloud_user_get', transform=transform_output)

    # TeamCloud Tags

    with self.command_group('tc tag', client_factory=teamcloud_client_factory) as g:
        g.custom_command('create', 'teamcloud_tag_create', transform=transform_output,
                         supports_no_wait=True)
        g.custom_command('delete', 'teamcloud_tag_delete', transform=transform_output,
                         supports_no_wait=True)
        g.custom_command('list', 'teamcloud_tag_list', transform=transform_output,
                         table_transformer=transform_tag_table_output)
        g.custom_show_command('show', 'teamcloud_tag_get', transform=transform_output)

    # Projects

    with self.command_group('tc project', client_factory=teamcloud_client_factory) as g:
        g.custom_command('create', 'project_create', transform=transform_output,
                         supports_no_wait=True)
        g.custom_command('delete', 'project_delete', transform=transform_output,
                         supports_no_wait=True, confirmation='Are you sure you want to delete this project?')
        g.custom_command('list', 'project_list', transform=transform_output,
                         table_transformer=transform_project_table_output)
        g.custom_show_command('show', 'project_get', transform=transform_output)

    # Project Users

    with self.command_group('tc project user', client_factory=teamcloud_client_factory) as g:
        g.custom_command('create', 'project_user_create', transform=transform_output,
                         supports_no_wait=True)
        g.custom_command('delete', 'project_user_delete', transform=transform_output,
                         supports_no_wait=True, confirmation='Are you sure you want to delete this user?')
        g.custom_command('list', 'project_user_list', transform=transform_output,
                         table_transformer=transform_user_table_output)
        g.custom_show_command('show', 'project_user_get', transform=transform_output)

    # Project Tags

    with self.command_group('tc project tag', client_factory=teamcloud_client_factory) as g:
        g.custom_command('create', 'project_tag_create', transform=transform_output,
                         supports_no_wait=True)
        g.custom_command('delete', 'project_tag_delete', transform=transform_output,
                         supports_no_wait=True)
        g.custom_command('list', 'project_tag_list', transform=transform_output,
                         table_transformer=transform_tag_table_output)
        g.custom_show_command('show', 'project_tag_get', transform=transform_output)

    # Project Types

    with self.command_group('tc project-type', client_factory=teamcloud_client_factory) as g:
        g.custom_command('create', 'project_type_create', transform=transform_output)
        g.custom_command('delete', 'project_type_delete', transform=transform_output,
                         confirmation='Are you sure you want to delete this project type?')
        g.custom_command('list', 'project_type_list', transform=transform_output,
                         table_transformer=transform_project_type_table_output)
        g.custom_show_command('show', 'project_type_get', transform=transform_output)

    # Providers

    with self.command_group('tc provider', client_factory=teamcloud_client_factory) as g:
        g.custom_command('create', 'provider_create', transform=transform_output,
                         supports_no_wait=True)
        g.custom_command('delete', 'provider_delete', transform=transform_output,
                         supports_no_wait=True, confirmation='Are you sure you want to delete this provider?')
        g.custom_command('list', 'provider_list', transform=transform_output,
                         table_transformer=transform_provider_table_output)
        g.custom_show_command('show', 'provider_get', transform=transform_output)
        g.custom_command('deploy', 'provider_deploy', transform=transform_output)
        g.custom_command('upgrade', 'provider_upgrade', transform=transform_output)
        g.custom_command('list-available', 'provider_list_available')

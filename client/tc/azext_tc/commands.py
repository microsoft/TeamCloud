# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

from ._client_factory import teamcloud_client_factory
from ._transformers import (transform_output, transform_org_table_output, transform_template_table_output,
                            transform_scope_table_output)
from ._validators import tc_deploy_validator


def load_command_table(self, _):  # pylint: disable=too-many-statements

    # TeamCloud

    with self.command_group('tc', is_preview=True):
        pass

    with self.command_group('tc', client_factory=teamcloud_client_factory) as g:
        g.custom_command('deploy', 'teamcloud_deploy', validator=tc_deploy_validator)
        g.custom_command('update', 'teamcloud_update')
        # g.custom_command('test', 'teamcloud_test', transform=transform_output)

    with self.command_group('tc app', client_factory=teamcloud_client_factory) as g:
        g.custom_command('deploy', 'teamcloud_app_deploy')

    # Orgs

    with self.command_group('tc org', client_factory=teamcloud_client_factory) as g:
        g.custom_command('create', 'org_create', transform=transform_output,
                         supports_no_wait=True)
        g.custom_command('delete', 'org_delete', transform=transform_output,
                         supports_no_wait=True, confirmation='Are you sure you want to delete this org?')
        g.custom_command('list', 'org_list', transform=transform_output,
                         table_transformer=transform_org_table_output)
        g.custom_show_command('show', 'org_get', transform=transform_output)

    # Deployment Scopes

    with self.command_group('tc scope', client_factory=teamcloud_client_factory) as g:
        g.custom_command('create', 'deployment_scope_create', transform=transform_output)
        g.custom_command('delete', 'deployment_scope_delete', transform=transform_output,
                         confirmation='Are you sure you want to delete this deployment scope?')
        g.custom_command('list', 'deployment_scope_list', transform=transform_output,
                         table_transformer=transform_scope_table_output)
        g.custom_show_command('show', 'deployment_scope_get', transform=transform_output)

    # Project Templates

    with self.command_group('tc template', client_factory=teamcloud_client_factory) as g:
        g.custom_command('create', 'project_template_create', transform=transform_output)
        g.custom_command('delete', 'project_template_delete', transform=transform_output,
                         confirmation='Are you sure you want to delete this project template?')
        g.custom_command('list', 'project_template_list', transform=transform_output,
                         table_transformer=transform_template_table_output)
        g.custom_show_command('show', 'project_template_get', transform=transform_output)

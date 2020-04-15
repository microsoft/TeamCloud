# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

# pylint: disable=too-many-statements
import platform

from knack.arguments import CLIArgumentType
from azure.cli.core.commands.parameters import (
    tags_type, get_enum_type, get_location_type, resource_group_name_type)

from ._validators import (
    project_name_validator, project_name_or_id_validator, user_name_validator, user_name_or_id_validator,
    tracking_id_validator, project_type_id_validator, project_type_id_validator_name, provider_id_validator,
    subscriptions_list_validator, provider_event_list_validator, url_validator, base_url_validator,
    source_version_validator, auth_code_validator, properties_validator)

from ._completers import (
    get_project_completion_list, get_project_type_completion_list, get_provider_completion_list)

from ._actions import CreateProviderReference


def load_arguments(self, _):

    quotes = '""' if platform.system() == 'Windows' else "''"

    tc_url_type = CLIArgumentType(
        options_list=['--base-url', '-u'],
        help='Base url of the TeamCloud instance. Use `az configure --defaults tc-base-url=<url>` '
             'to configure a default.',
        configured_default='tc-base-url',
        validator=base_url_validator)

    user_name_type = CLIArgumentType(
        options_list=['--name', '-n'],
        help='User email.',
        validator=user_name_validator)

    user_name_or_id_type = CLIArgumentType(
        options_list=['--name', '-n'],
        help='User id (uuid) or email.',
        validator=user_name_or_id_validator)

    project_name_or_id_type = CLIArgumentType(
        options_list=['--project', '-p'],
        help='Project id (uuid) or name.',
        validator=project_name_or_id_validator,
        completer=get_project_completion_list)

    properties_type = CLIArgumentType(
        validator=properties_validator,
        help='Space-separated properties: key[=value] [key[=value] ...].'
             'Use {} to clear existing properties.'.format(quotes),
        nargs='*')

    # Global

    # ignore global az arg --subscription and requre base_url for everything except `tc deploy`
    for scope in ['tc status', 'tc upgrade', 'tc user', 'tc project', 'tc project-type', 'tc provider', 'tc tag']:
        with self.argument_context(scope, arg_group='TeamCloud') as c:
            c.ignore('_subscription')
            c.argument('base_url', tc_url_type)

    # Tags

    for scope in ['tc tag create', 'tc tag show', 'tc tag delete',
                  'tc project tag create', 'tc project tag show', 'tc project tag delete']:
        with self.argument_context(scope) as c:
            c.argument('tag_key', options_list=['--key', '-k'], help='Tag key.')

    for scope in ['tc tag create', 'tc project tag create']:
        with self.argument_context(scope) as c:
            c.argument('tag_value', options_list=['--value', '-v'], help='Tag value.')

    with self.argument_context('tc project tag') as c:
        c.argument('project', project_name_or_id_type)

    # TeamCloud

    # tc deploy uses a command level validator, param validators will be ignored
    with self.argument_context('tc deploy') as c:
        c.argument('name', options_list=['--name', '-n'],
                   help='Name of app. Must be globally unique and will be the subdomain '
                        'for the TeamCloud instance service endpoint.')
        c.argument('resource_group_name', resource_group_name_type,
                   default='TeamCloud')
        c.argument('principal_name', help='Service principal name, or object id.')
        c.argument('principal_password', help="Service principal password, aka 'client secret'.")
        c.argument('tags', tags_type)
        c.argument('version', options_list=['--version', '-v'],
                   help='TeamCloud version. Default: latest stable.')
        c.argument('skip_app_deployment', action='store_true',
                   help="Only create Azure resources, skip deploying the TeamCloud API and Orchestrator apps.")
        c.argument('skip_name_validation', action='store_true',
                   help="Skip name validaiton. Useful when attempting to redeploy a partial or failed deployment.")
        c.argument('skip_admin_user', action='store_true',
                   help="Skip adding Admin user.")

    with self.argument_context('tc upgrade') as c:
        c.argument('resource_group_name', resource_group_name_type, default='TeamCloud')
        c.argument('version', options_list=['--version', '-v'], help='TeamCloud version. Default: latest stable.',
                   validator=source_version_validator)

    with self.argument_context('tc status') as c:
        c.argument('project', project_name_or_id_type)
        c.argument('tracking_id', options_list=['--tracking-id', '-t'],
                   type=str, help='Operation tracking id.',
                   validator=tracking_id_validator)

    # TeamCloud Users

    with self.argument_context('tc user create') as c:
        c.argument('user_name', user_name_type)
        c.argument('user_role', get_enum_type(['Admin', 'Creator'], default='Creator'),
                   options_list=['--role', '-r'], help='User role.')
        c.argument('tags', tags_type)

    for scope in ['tc user show', 'tc user delete']:
        with self.argument_context(scope) as c:
            c.argument('user', user_name_or_id_type)

    # Projects

    with self.argument_context('tc project create') as c:
        c.argument('name', options_list=['--name', '-n'],
                   type=str, help='Project name.',
                   validator=project_name_validator)
        c.argument('project_type', options_list=['--project-type', '-t'],
                   type=str, help='Project type id. Use `az tc project-type list` to get available project types',
                   validator=project_type_id_validator)
        c.argument('tags', tags_type)

    for scope in ['tc project show', 'tc project delete']:
        with self.argument_context(scope) as c:
            c.argument('project', options_list=['--name', '-n'],
                       type=str, help='Project name or id (uuid).',
                       validator=project_name_or_id_validator,
                       completer=get_project_completion_list)

    # Project Users

    with self.argument_context('tc project user') as c:
        c.argument('project', project_name_or_id_type)

    with self.argument_context('tc project user create') as c:
        c.argument('user_name', user_name_type)
        c.argument('user_role', get_enum_type(['Owner', 'Member'], default='Member'),
                   options_list=['--role', '-r'], help='User role.')
        c.argument('tags', tags_type)

    for scope in ['tc project user show', 'tc project user delete']:
        with self.argument_context(scope) as c:
            c.argument('user', user_name_or_id_type)

    # Project Types

    with self.argument_context('tc project-type create') as c:
        c.argument('project_type', options_list=['--name', '-n'],
                   type=str, help='Project type id.',
                   validator=project_type_id_validator_name)
        c.argument('location', get_location_type(self.cli_ctx),
                   help='Project type region.')
        c.argument('subscriptions', nargs='+',
                   help='Space-seperated subscription ids (uuids).',
                   validator=subscriptions_list_validator)
        c.argument('subscription_capacity', type=int, default=10,
                   help='Maximum number of projects per subscription.')
        c.argument('default', action='store_true',
                   help='Set as the default project type.')
        c.argument('resource_group_name_prefix', type=str,
                   help='Prepended to all project resource group names.')
        c.argument('provider', nargs='+', action=CreateProviderReference,
                   help='Project type provider: provider_id [key=value ...]. '
                   'Use depends_on key to define dependencies. '
                   'Use multiple --provider arguemnts to specify additional providers.')
        c.argument('tags', tags_type)
        c.argument('properties', properties_type)
        c.ignore('providers')

    for scope in ['tc project-type show', 'tc project-type delete']:
        with self.argument_context(scope) as c:
            c.argument('project_type', options_list=['--name', '-n'],
                       type=str, help='Project type id.',
                       validator=project_type_id_validator_name,
                       completer=get_project_type_completion_list)

    # Providers

    with self.argument_context('tc provider create') as c:
        c.argument('provider', options_list=['--name', '-n'],
                   type=str, help='Provider id.',
                   validator=provider_id_validator)

    for scope in ['tc provider show', 'tc provider delete']:
        with self.argument_context(scope) as c:
            c.argument('provider', options_list=['--name', '-n'],
                       type=str, help='Provider id.',
                       validator=provider_id_validator,
                       completer=get_provider_completion_list)

    for scope in ['tc provider create', 'tc provider deploy']:
        with self.argument_context(scope) as c:
            c.argument('events', nargs='+',
                       help='Space-seperated provider ids.',
                       validator=provider_event_list_validator)
            c.argument('properties', properties_type)

    with self.argument_context('tc provider create') as c:
        c.argument('url', type=str, help='Provider url.',
                   validator=url_validator)
        c.argument('auth_code', type=str, help='Provider auth code.',
                   validator=auth_code_validator)

    for scope in ['tc provider deploy', 'tc provider upgrade']:
        with self.argument_context(scope) as c:
            c.argument('provider', get_enum_type(['azure.appinsights', 'azure.devops', 'azure.devtestlabs']),
                       options_list=['--name', '-n'], help='Provider id.')
            c.argument('resource_group_name', resource_group_name_type,
                       help='Name of resource group.')
            c.argument('version', options_list=['--version', '-v'],
                       type=str, help='Provider version. Default: latest stable.',
                       validator=source_version_validator)

    with self.argument_context('tc provider deploy') as c:
        c.argument('location', get_location_type(self.cli_ctx))
        c.argument('tags', tags_type)

# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

# pylint: disable=too-many-statements
from knack.arguments import CLIArgumentType
from azure.cli.core.commands.parameters import (tags_type, get_enum_type)

from ._validators import (
    org_name_or_id_validator, org_name_validator, base_url_validator,
    teamcloud_cli_source_version_validator, repo_url_validator,
    teamcloud_source_version_validator, index_url_validator, client_id_validator)

from ._completers import (get_org_completion_list)


def load_arguments(self, _):

    tc_url_type = CLIArgumentType(
        options_list=['--url', '-u'],
        help='Base url of the TeamCloud instance. Use `az configure -d tc-url=<url>` '
             'to configure a default.',
        configured_default='tc-url',
        validator=base_url_validator)

    org_name_or_id_type = CLIArgumentType(
        options_list=['--org'],
        help='Organization id (uuid) or name. Use `az configure -d tc-org=<url>` '
             'to configure a default.',
        configured_default='tc-org',
        validator=org_name_or_id_validator)

    parameters_type = CLIArgumentType(
        options_list=['--parameters', '-p'],
        action='append',
        nargs='+',
        help='the deployment parameters')

    # with self.argument_context('tc test', arg_group='TeamCloud') as c:
    #     c.argument('base_url', tc_url_type)
    #     c.argument('scope', options_list=['--name', '-n'],
    #                type=str, help='Deployment scope name.')
    #     c.argument('scope_type', get_enum_type(['AzureResourceManager', 'GitHub', 'AzureDevOps'],
    #                default='AzureResourceManager'),
    #                options_list=['--type', '-t'], help='Deployment scope name.')
    #     c.argument('parameters', arg_type=parameters_type)
    #     c.ignore('_subscription')

    # Global

    # ignore global az arg --subscription and requre base_url for everything except `tc deploy`
    for scope in ['tc update', 'tc app', 'tc org', 'tc template', 'tc scope']:
        with self.argument_context(scope, arg_group='TeamCloud') as c:
            c.argument('base_url', tc_url_type)

    for scope in ['tc update', 'tc org delete', 'tc org list', 'tc org show', 'tc template', 'tc scope']:
        with self.argument_context(scope, arg_group='TeamCloud') as c:
            c.ignore('_subscription')

    for scope in ['tc template', 'tc scope']:
        with self.argument_context(scope, arg_group='TeamCloud') as c:
            c.argument('org', org_name_or_id_type)

    # TeamCloud CLI

    with self.argument_context('tc update') as c:
        c.argument('version', options_list=['--version', '-v'], help='TeamCloud version (tag). Default: latest stable.',
                   validator=teamcloud_cli_source_version_validator)
        c.argument('prerelease', options_list=['--pre'], action='store_true',
                   help='Deploy latest prerelease version.')

    # tc deploy uses a command level validator, param validators will be ignored
    with self.argument_context('tc deploy') as c:
        c.argument('name', options_list=['--name', '-n'],
                   help='Name of app. Must be globally unique and will be the subdomain '
                        'for the TeamCloud instance service endpoint.')
        c.argument('principal_name', help='Service principal app (client) id.')
        c.argument('principal_password', help="Service principal password, aka 'client secret'.")
        c.argument('tags', tags_type)
        c.argument('version', options_list=['--version', '-v'],
                   help='TeamCloud version. Default: latest stable.')
        c.argument('prerelease', options_list=['--pre'], action='store_true',
                   help='Deploy latest prerelease version.')
        c.argument('skip_app_deployment', action='store_true',
                   help="Only create Azure resources, skip deploying the TeamCloud API and Orchestrator apps.")
        c.argument('skip_name_validation', action='store_true',
                   help="Skip name validaiton. Useful when attempting to redeploy a partial or failed deployment.")
        c.argument('index_url', help='URL to custom index.json file.')

    with self.argument_context('tc app deploy') as c:
        c.argument('client_id', options_list=['--client-id', '-c'],
                   type=str, validator=client_id_validator,
                   help='Client ID for the Managed Application used for user authentication. '
                   'See https://aka.ms/tcwebclientid for instructions.')
        c.argument('app_type', get_enum_type(['Web'], default='Web'),
                   options_list=['--type', '-t'], help='App type. Currently only supports Web')
        c.argument('version', options_list=['--version', '-v'],
                   type=str, help='App version. Default: latest stable.',
                   validator=teamcloud_source_version_validator)
        c.argument('prerelease', options_list=['--pre'], action='store_true',
                   help='Deploy latest prerelease version.')
        c.argument('index_url', help='URL to custom index.json file.',
                   validator=index_url_validator)
        c.argument('scope', help='Scope to use for user authentication.')

    # Orgs

    with self.argument_context('tc org create') as c:
        c.argument('name', options_list=['--org', '--name', '-n'],
                   type=str, help='Organization name.',
                   validator=org_name_validator)

    for scope in ['tc org show', 'tc org delete']:
        with self.argument_context(scope) as c:
            c.argument('org', options_list=['--name', '--org', '-n'],
                       type=str, help='Organization name or id (uuid).',
                       validator=org_name_or_id_validator,
                       completer=get_org_completion_list)

    # Deployment Scopes

    with self.argument_context('tc scope create') as c:
        c.argument('scope', options_list=['--name', '-n'],
                   type=str, help='Deployment scope name.')
        c.argument('scope_type', get_enum_type(['AzureResourceManager', 'GitHub', 'AzureDevOps'],
                   default='AzureResourceManager'),
                   options_list=['--type', '-t'], help='Deployment scope name.')
        c.argument('parameters', arg_type=parameters_type)

    for scope in ['tc scope show', 'tc scope delete']:
        with self.argument_context(scope) as c:
            c.argument('scope', options_list=['--name', '-n'],
                       type=str, help='Deployment scope name or id (uuid).')

    # Project Templates

    with self.argument_context('tc template create') as c:
        c.argument('template', options_list=['--name', '-n'],
                   type=str, help='Project template name.')
        c.argument('repo_url', options_list=['--repo-url', '-r'],
                   help='URL to the project template repo.',
                   validator=repo_url_validator)
        c.argument('repo_version', options_list=['--repo-version', '-v'],
                   help='Version (tag, branch, or ref). Default: latest stable.')
        c.argument('repo_token', options_list=['--repo-token', '-t'],
                   help='Personal access token.')

    for scope in ['tc template show', 'tc template delete']:
        with self.argument_context(scope) as c:
            c.argument('template', options_list=['--name', '-n'],
                       type=str, help='Project template name or id (uuid).')

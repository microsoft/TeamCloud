# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

import os
import unittest

from azure_devtools.scenario_tests import AllowLargeResponse
from azure.cli.testsdk import (ScenarioTest, ResourceGroupPreparer,
                               RoleBasedServicePrincipalPreparer)


TEST_DIR = os.path.abspath(os.path.join(os.path.abspath(__file__), '..'))


class TeamCloudScenarioTest(ScenarioTest):

    @AllowLargeResponse()
    @RoleBasedServicePrincipalPreparer()
    @ResourceGroupPreparer(parameter_name='tc_group', parameter_name_for_location='location', location='eastus', key='rg')
    @ResourceGroupPreparer(parameter_name='ai_group', location='eastus', key='rg_ai')
    @ResourceGroupPreparer(parameter_name='dtl_group', location='eastus', key='rg_dtl')
    @ResourceGroupPreparer(parameter_name='ado_group', location='eastus', key='rg_ado')
    @ResourceGroupPreparer(parameter_name='gh_group', location='eastus', key='rg_gh')
    def test_tc(self, sp_name, sp_password, tc_group, ai_group, dtl_group, ado_group, gh_group, location):

        subs = self.cmd('az account show', checks=[
            self.exists('id'),
            self.exists('user.name')
        ]).get_output_in_json()

        self.kwargs.update({
            'tc': self.create_random_name(prefix='cli', length=11),
            'proj': self.create_random_name(prefix='cli', length=11),
            'loc': location,
            'sub': subs['id'],
            'username': subs['user']['name'],
            'prop_k': 'CLIProperty',
            'prop_v': 'CLIPropertyValue',
            'proj_type': 'cli.test',
            'ai_provider': 'azure.appinsights',
            'dtl_provider': 'azure.devtestlabs',
            'ado_provider': 'azure.devops',
            'gh_provider': 'github'
        })

        ci = os.environ.get('AZURE_CLI_TEST_DEV_SP_NAME', None)

        result = self.cmd('tc deploy -n {tc} -g {rg} -l {loc} --pre' +
                          (' --principal-name {sp} --principal-password {sp_pass}' if ci else ''),
                          checks=[
                              self.check('location', '{loc}'),
                              self.exists('base_url')
                          ]).get_output_in_json()

        self.kwargs.update({'url': result['base_url']})

        # add admin user may not be complete
        if self.is_live:
            import time
            time.sleep(120)  # wait 2 minutes before continuing

        result = self.cmd('tc user show -u {url} -n {username}', checks=[
            self.exists('id'),
            self.check('role', 'Admin')
        ]).get_output_in_json()

        self.kwargs.update({'user': result['id']})

        self.cmd('tc user list -u {url}', checks=[
            self.check('type(@)', 'array'),
            self.check('length(@)', 1),
            self.check('[0].id', '{user}'),
            self.check('[0].role', 'Admin'),
        ])

        self.cmd('tc user update -u {url} -n {username} --properties {prop_k}={prop_v}', checks=[
            self.check('id', '{user}'),
            self.check('role', 'Admin'),
            self.check('properties.{prop_k}', '{prop_v}')
        ])

        self.cmd('tc provider deploy -u {url} -n {ai_provider} -g {rg_ai} --pre', checks=[
            self.check('id', '{ai_provider}'),
            self.exists('url'),
            self.exists('registered')
        ])

        self.cmd('tc provider show -u {url} -n {ai_provider}', checks=[
            self.check('id', '{ai_provider}'),
            self.exists('url'),
            self.exists('registered')
        ])

        self.cmd('tc provider deploy -u {url} -n {dtl_provider} -g {rg_dtl} --pre', checks=[
            self.check('id', '{dtl_provider}'),
            self.exists('url'),
            self.exists('registered')
        ])

        self.cmd('tc provider show -u {url} -n {dtl_provider}', checks=[
            self.check('id', '{dtl_provider}'),
            self.exists('url'),
            self.exists('registered')
        ])

        self.cmd('tc provider deploy -u {url} -n {ado_provider} -g {rg_ado} --pre', checks=[
            self.check('id', '{ado_provider}'),
            self.exists('url'),
            self.exists('registered')
        ])

        self.cmd('tc provider show -u {url} -n {ado_provider}', checks=[
            self.check('id', '{ado_provider}'),
            self.exists('url'),
            self.exists('registered')
        ])

        self.cmd('tc provider deploy -u {url} -n {gh_provider} -g {rg_gh} --pre', checks=[
            self.check('id', '{gh_provider}'),
            self.exists('url'),
            self.exists('registered')
        ])

        self.cmd('tc provider show -u {url} -n {gh_provider}', checks=[
            self.check('id', '{gh_provider}'),
            self.exists('url'),
            self.exists('registered')
        ])

        self.cmd('tc provider list -u {url}', checks=[
            self.check('type(@)', 'array'),
            self.check('length(@)', 2),
            self.check("contains([].id, '{ai_provider}')", True),
            self.exists("[?id=='{ai_provider}'] | [0].url"),
            self.exists("[?id=='{ai_provider}'] | [0].registered"),
            self.check("contains([].id, '{dtl_provider}')", True),
            self.exists("[?id=='{dtl_provider}'] | [0].url"),
            self.exists("[?id=='{dtl_provider}'] | [0].registered"),
            self.check("contains([].id, '{ado_provider}')", True),
            self.exists("[?id=='{ado_provider}'] | [0].url"),
            self.exists("[?id=='{ado_provider}'] | [0].registered"),
            self.check("contains([].id, '{gh_provider}')", True),
            self.exists("[?id=='{gh_provider}'] | [0].url"),
            self.exists("[?id=='{gh_provider}'] | [0].registered"),
        ])

        self.cmd('tc project-type create -u {url} -n {proj_type} -l {loc} '
                 '--subscriptions {sub} --resource-group-name-prefix clitest.rg '
                 '--provider {ai_provider} --provider {dtl_provider}', checks=[
                     self.check('id', '{proj_type}'),
                     self.check('region', '{loc}'),
                     self.check('length(providers)', 2),
                     self.check("contains(providers[].id, '{ai_provider}')", True),
                     self.check("contains(providers[].id, '{dtl_provider}')", True),
                 ])

        self.cmd('tc project-type show -u {url} -n {proj_type}', checks=[
            self.check('id', '{proj_type}'),
            self.check('region', '{loc}'),
            self.check('length(providers)', 2),
            self.check("contains(providers[].id, '{ai_provider}')", True),
            self.check("contains(providers[].id, '{dtl_provider}')", True),
        ])

        self.cmd('tc project-type list -u {url}', checks=[
            self.check('type(@)', 'array'),
            self.check('length(@)', 1),
            self.check('[0].id', '{proj_type}'),
            self.check('[0].region', '{loc}'),
            self.check('length([0].providers)', 2),
            self.check("contains([0].providers[].id, '{ai_provider}')", True),
            self.check("contains([0].providers[].id, '{dtl_provider}')", True),
        ])

        result = self.cmd('tc project create -u {url} -n {proj} -t {proj_type}', checks=[
            self.exists('id'),
            self.check('name', '{proj}'),
            self.check('type.id', '{proj_type}'),
            self.check('length(type.providers)', 2),
            self.check("contains(type.providers[].id, '{ai_provider}')", True),
            self.check("contains(type.providers[].id, '{dtl_provider}')", True),
            self.check('length(users)', 1),
            self.check('users[0].id', '{user}'),
            self.check('length(users[0].projectMemberships)', 1),
            self.exists('users[0].projectMemberships[0].projectId'),
            self.check('users[0].projectMemberships[0].role', 'Owner')
        ]).get_output_in_json()

        self.kwargs.update({'proj_id': result['id']})

        self.cmd('tc project show -u {url} -n {proj}', checks=[
            self.check('id', '{proj_id}'),
            self.check('name', '{proj}'),
            self.check('type.id', '{proj_type}'),
            self.check('length(type.providers)', 2),
            self.check("contains(type.providers[].id, '{ai_provider}')", True),
            self.check("contains(type.providers[].id, '{dtl_provider}')", True),
            self.check('length(users)', 1),
            self.check('users[0].id', '{user}'),
            self.check('length(users[0].projectMemberships)', 1),
            self.check('users[0].projectMemberships[0].projectId', '{proj_id}'),
            self.check('users[0].projectMemberships[0].role', 'Owner')
        ])

        self.cmd('tc project list -u {url}', checks=[
            self.check('type(@)', 'array'),
            self.check('length(@)', 1),
            self.check('[0].id', '{proj_id}'),
            self.check('[0].name', '{proj}'),
            self.check('[0].type.id', '{proj_type}'),
            self.check('length([0].type.providers)', 2),
            self.check("contains([0].type.providers[].id, '{ai_provider}')", True),
            self.check("contains([0].type.providers[].id, '{dtl_provider}')", True),
            self.check('length([0].users)', 1),
            self.check('[0].users[0].id', '{user}'),
            self.check('length([0].users[0].projectMemberships)', 1),
            self.check('[0].users[0].projectMemberships[0].projectId', '{proj_id}'),
            self.check('[0].users[0].projectMemberships[0].role', 'Owner')
        ])

        self.cmd('tc project user update -u {url} -p {proj} -n {username} '
                 '--properties {prop_k}={prop_v}', checks=[
                     self.check('id', '{user}'),
                     self.check('role', 'Admin'),
                     self.check('properties.{prop_k}', '{prop_v}'),
                     self.check('length(projectMemberships)', 1),
                     self.check('projectMemberships[0].projectId', '{proj_id}'),
                     self.check('projectMemberships[0].role', 'Owner'),
                     self.check('projectMemberships[0].properties.{prop_k}', '{prop_v}')
                 ])

        self.cmd('tc project delete -u {url} -n {proj} -y', checks=[
            self.check('code', 200),
            self.check('state', 'Completed'),
            self.check('status', 'Ok'),
        ])

        self.cmd('tc project list -u {url}', checks=self.is_empty())

        self.cmd('tc user show -u {url} -n {username}', checks=[
            self.check('id', '{user}'),
            self.check('role', 'Admin'),
            self.check('properties.{prop_k}', '{prop_v}'),
            self.check('length(projectMemberships)', 0),
        ])

        self.cmd('tc project-type delete -u {url} -n {proj_type} -y', checks=self.is_empty())

        self.cmd('tc project-type list -u {url}', checks=self.is_empty())

        # give the orchestrator time to clean up project rgs
        if self.is_live:
            import time
            time.sleep(300)  # wait 5 minutes before completing

# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

import os
import unittest

from azure_devtools.scenario_tests import AllowLargeResponse
from azure.cli.testsdk import (ScenarioTest, ResourceGroupPreparer)


TEST_DIR = os.path.abspath(os.path.join(os.path.abspath(__file__), '..'))


class TeamCloudScenarioTest(ScenarioTest):

    @AllowLargeResponse()
    @ResourceGroupPreparer(parameter_name='tc_group', parameter_name_for_location='location', location='eastus')
    @ResourceGroupPreparer(parameter_name='provider_group_appinsights', location='eastus')
    @ResourceGroupPreparer(parameter_name='provider_group_devtestlabs', location='eastus')
    def test_tc(self, tc_group, provider_group_appinsights, provider_group_devtestlabs, location):

        subs = self.cmd('az account show').get_output_in_json()
        subscription = subs['id']
        user_email = subs['user']['name']

        tc_name = self.create_random_name(prefix='cli', length=11)

        # cmd = 'tc deploy -n {tc_name} -g {tc_group} -l {location}'.format(**locals())
        cmd = 'tc deploy -n {tc_name} -g {tc_group} -l {location} --pre'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(result['location'], location)
        self.assertIsNotNone(result['base_url'])

        base_url = result['base_url']

        # add admin user may not be complete
        if self.is_live:
            import time
            time.sleep(120)  # wait 2 minutes before continuing

        cmd = 'tc user show -u {base_url} -n {user_email}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertIsNotNone(result['id'])
        self.assertEqual(result['role'], 'Admin')

        user = result['id']
        user_prop_key = 'CLIProperty'
        user_prop_val = 'CLIPropertyValue'

        cmd = 'tc user list -u {base_url}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertGreaterEqual(len(result), 1)
        index = next((i for i, p in enumerate(result) if p['id'] == user), None)
        self.assertIsNotNone(index)

        cmd = 'tc user update -u {base_url} -n {user_email} --properties {user_prop_key}={user_prop_val}'.format(
            **locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertIsNotNone(result['id'])
        self.assertEqual(result['role'], 'Admin')
        self.assertEqual(result['properties'][user_prop_key], user_prop_val)

        appinsights_provider = 'azure.appinsights'

        cmd = 'tc provider deploy -u {base_url} -n {appinsights_provider} -g {provider_group_appinsights} --pre'.format(
            **locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(result['id'], appinsights_provider)
        self.assertIsNotNone(result['url'])

        cmd = 'tc provider show -u {base_url} -n {appinsights_provider}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(result['id'], appinsights_provider)
        self.assertIsNotNone(result['url'])

        devtestlabs_provider = 'azure.devtestlabs'

        cmd = 'tc provider deploy -u {base_url} -n {devtestlabs_provider} -g {provider_group_devtestlabs} --pre'.format(
            **locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(result['id'], devtestlabs_provider)
        self.assertIsNotNone(result['url'])

        cmd = 'tc provider show -u {base_url} -n {devtestlabs_provider}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(result['id'], devtestlabs_provider)
        self.assertIsNotNone(result['url'])

        cmd = 'tc provider list -u {base_url}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertGreaterEqual(len(result), 1)
        index = next((i for i, p in enumerate(result)
                      if p['id'] == appinsights_provider), None)
        self.assertIsNotNone(index)
        self.assertIsNotNone(result[index]['url'])
        index = next((i for i, p in enumerate(result)
                      if p['id'] == devtestlabs_provider), None)
        self.assertIsNotNone(index)
        self.assertIsNotNone(result[index]['url'])

        project_type = 'cli.test'

        cmd = 'tc project-type create -u {base_url} -n {project_type} -l {location} --subscriptions {subscription} --resource-group-name-prefix clitest.rg --provider {appinsights_provider} --provider {devtestlabs_provider}'.format(
            **locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(result['id'], project_type)
        self.assertEqual(result['region'], location)
        self.assertGreaterEqual(len(result['providers']), 1)
        index = next((i for i, p in enumerate(result['providers'])
                      if p['id'] == appinsights_provider), None)
        self.assertIsNotNone(index)
        index = next((i for i, p in enumerate(result['providers'])
                      if p['id'] == devtestlabs_provider), None)
        self.assertIsNotNone(index)

        cmd = 'tc project-type show -u {base_url} -n {project_type}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(result['id'], project_type)
        self.assertEqual(result['region'], location)
        self.assertGreaterEqual(len(result['providers']), 1)
        index = next((i for i, p in enumerate(result['providers'])
                      if p['id'] == appinsights_provider), None)
        self.assertIsNotNone(index)
        index = next((i for i, p in enumerate(result['providers'])
                      if p['id'] == devtestlabs_provider), None)
        self.assertIsNotNone(index)

        cmd = 'tc project-type list -u {base_url}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertGreaterEqual(len(result), 1)
        index = next((i for i, p in enumerate(result) if p['id'] == project_type), None)
        self.assertIsNotNone(index)
        self.assertEqual(result[index]['region'], location)
        self.assertGreaterEqual(len(result[index]['providers']), 1)
        sub_index = next((i for i, p in enumerate(result[index]['providers'])
                          if p['id'] == appinsights_provider), None)
        self.assertIsNotNone(sub_index)
        sub_index = next((i for i, p in enumerate(result[index]['providers'])
                          if p['id'] == devtestlabs_provider), None)
        self.assertIsNotNone(sub_index)

        project = self.create_random_name(prefix='cli', length=11)

        cmd = 'tc project create -u {base_url} -n {project} -t {project_type}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertIsNotNone(result['id'])
        self.assertEqual(result['name'], project)
        self.assertEqual(result['type']['id'], project_type)
        self.assertGreaterEqual(len(result['type']['providers']), 1)
        index = next((i for i, p in enumerate(result['type']['providers'])
                      if p['id'] == appinsights_provider), None)
        self.assertIsNotNone(index)
        index = next((i for i, p in enumerate(result['type']['providers'])
                      if p['id'] == devtestlabs_provider), None)
        self.assertIsNotNone(index)
        self.assertGreaterEqual(len(result['users']), 1)
        index = next((i for i, p in enumerate(result['users']) if p['id'] == user), None)
        self.assertIsNotNone(index)
        sub_index = next((i for i, p in enumerate(result['users'][index]['projectMemberships'])
                          if p['projectId'] == result['id']), None)
        self.assertIsNotNone(sub_index)
        self.assertEqual(result['users'][index]['projectMemberships'][sub_index]['role'], 'Owner')

        project_id = result['id']

        cmd = 'tc project show -u {base_url} -n {project}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(result['name'], project)
        self.assertEqual(result['type']['id'], project_type)
        self.assertGreaterEqual(len(result['type']['providers']), 1)
        index = next((i for i, p in enumerate(result['type']['providers'])
                      if p['id'] == appinsights_provider), None)
        self.assertIsNotNone(index)
        index = next((i for i, p in enumerate(result['type']['providers'])
                      if p['id'] == devtestlabs_provider), None)
        self.assertIsNotNone(index)
        self.assertGreaterEqual(len(result['users']), 1)
        index = next((i for i, p in enumerate(result['users']) if p['id'] == user), None)
        self.assertIsNotNone(index)
        sub_index = next((i for i, p in enumerate(result['users'][index]['projectMemberships'])
                          if p['projectId'] == project_id), None)
        self.assertIsNotNone(sub_index)
        self.assertEqual(result['users'][index]['projectMemberships'][sub_index]['role'], 'Owner')

        cmd = 'tc project list -u {base_url}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertGreaterEqual(len(result), 1)
        index = next((i for i, p in enumerate(result) if p['name'] == project), None)
        self.assertIsNotNone(index)
        self.assertEqual(result[index]['name'], project)
        self.assertEqual(result[index]['type']['id'], project_type)
        self.assertGreaterEqual(len(result[index]['type']['providers']), 1)
        sub_index = next((i for i, p in enumerate(result[index]['type']['providers'])
                          if p['id'] == appinsights_provider), None)
        self.assertIsNotNone(sub_index)
        sub_index = next((i for i, p in enumerate(result[index]['type']['providers'])
                          if p['id'] == devtestlabs_provider), None)
        self.assertIsNotNone(sub_index)
        sub_index = next((i for i, p in enumerate(result[index]['users'])
                          if p['id'] == user), None)
        self.assertIsNotNone(sub_index)
        sub_sub_index = next((i for i, p in enumerate(result[index]['users'][sub_index]['projectMemberships'])
                              if p['projectId'] == project_id), None)
        self.assertIsNotNone(sub_sub_index)
        self.assertEqual(result[index]['users'][sub_index]
                         ['projectMemberships'][sub_sub_index]['role'], 'Owner')

        cmd = 'tc project user update -u {base_url} -p {project} -n {user_email} --properties {user_prop_key}={user_prop_val}'.format(
            **locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertIsNotNone(result['id'])
        self.assertEqual(result['role'], 'Admin')
        self.assertEqual(result['properties'][user_prop_key], user_prop_val)
        index = next((i for i, p in enumerate(result['projectMemberships'])
                      if p['projectId'] == project_id), None)
        self.assertIsNotNone(index)
        self.assertEqual(result['projectMemberships'][index]['role'], 'Owner')
        self.assertEqual(result['projectMemberships'][index]
                         ['properties'][user_prop_key], user_prop_val)

        cmd = 'tc project delete -u {base_url} -n {project} -y'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()

        cmd = 'tc project list -u {base_url}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        index = next((i for i, p in enumerate(result) if p['name'] == project), None)
        self.assertIsNone(index)

        cmd = 'tc project-type delete -u {base_url} -n {project_type} -y'.format(**locals())
        result = self.cmd(cmd)

        cmd = 'tc project-type list -u {base_url}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        index = next((i for i, p in enumerate(result) if p['id'] == project_type), None)
        self.assertIsNone(index)

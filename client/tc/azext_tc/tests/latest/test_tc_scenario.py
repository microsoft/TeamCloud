# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

import os
import unittest

from azure_devtools.scenario_tests import AllowLargeResponse
from azure.cli.testsdk import (ScenarioTest, ResourceGroupPreparer)


TEST_DIR = os.path.abspath(os.path.join(os.path.abspath(__file__), '..'))


# class TeamCloudLiveScenarioTest(LiveScenarioTest):

#     def test_tc_project(self):
#         self.kwargs.update({
#             'url': 'http://localhost:5001',
#             'name': self.create_random_name(prefix='cli', length=10)
#         })

#         project_name = self.create_random_name(prefix='cli', length=10)

#         self.cmd('az tc project create -u {url} -n {name} --tags foo=bar', checks=[
#             self.check('tags.foo', 'bar'),
#             self.check('name', '{name}')
#         ]

class TeamCloudScenarioTest(ScenarioTest):

    @ResourceGroupPreparer(parameter_name='tc_group', parameter_name_for_location='location', location='eastus')
    @ResourceGroupPreparer(parameter_name='provider_group', location='eastus')
    def test_tc(self, tc_group, provider_group, location):

        pre_version = 'v0.1.288'
        provider_pre_version = 'v0.1.13'

        subs = self.cmd('az account show').get_output_in_json()
        subscription = subs['id']
        user_email = subs['user']['name']

        # tc_name = self.create_random_name(prefix='cli', length=11)

        # cmd = 'tc deploy -n {tc_name} -g {tc_group} -l {location} -v {pre_version}'.format(
        #     **locals())
        # result = self.cmd(cmd).get_output_in_json()
        # self.assertEqual(result['location'], location)
        # self.assertEqual(result['version'], pre_version)
        # self.assertIsNotNone(result['base_url'])

        # base_url = result['base_url']

        # # add admin user may not be complete
        # if self.is_live:
        #     import time
        #     time.sleep(120)  # wait 2 minutes before continuing

        base_url = 'https://tctesttest.azurewebsites.net'

        cmd = 'tc user list -u {base_url}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(len(result), 1)
        self.assertIsNotNone(result[0]['id'])

        user = result[0]['id']

        cmd = 'tc user show -u {base_url} -n {user}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(result['id'], user)

        provider = 'azure.appinsights'

        cmd = 'tc provider deploy -u {base_url} -n {provider} -g {provider_group} -v {provider_pre_version}'.format(
            **locals())
        result = self.cmd(cmd).get_output_in_json()
        # print(result)
        # self.assertEqual(result['id'], provider)
        # self.assertIsNotNone(result['url'])

        cmd = 'tc provider show -u {base_url} -n {provider}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(result['id'], provider)
        self.assertIsNotNone(result['url'])

        cmd = 'tc provider list -u {base_url}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(len(result), 1)
        self.assertEqual(result[0]['id'], provider)
        self.assertIsNotNone(result[0]['url'])

        project_type = 'cli.test.type'

        cmd = 'tc project-type create -u {base_url} -n {project_type} -l {location} --subscriptions {subscription} --resource-group-name-prefix CLI_ --provider {provider} --default'.format(
            **locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(result['id'], project_type)
        self.assertEqual(result['region'], location)
        self.assertTrue(result['default'])
        self.assertEqual(len(result['providers']), 1)
        self.assertEqual(result['providers'][0]['id'], provider)

        cmd = 'tc project-type show -u {base_url} -n {project_type}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(result['id'], project_type)
        self.assertEqual(result['region'], location)
        self.assertTrue(result['default'])
        self.assertEqual(len(result['providers']), 1)
        self.assertEqual(result['providers'][0]['id'], provider)

        cmd = 'tc project-type list -u {base_url}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        self.assertEqual(len(result), 1)
        self.assertEqual(result[0]['id'], project_type)
        self.assertEqual(result[0]['region'], location)
        self.assertTrue(result[0]['default'])
        self.assertEqual(len(result[0]['providers']), 1)
        self.assertEqual(result[0]['providers'][0]['id'], provider)

        project = self.create_random_name(prefix='cli', length=11)

        cmd = 'tc project create -u {base_url} -n {project}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        print(result)
        self.assertEqual(result['name'], project)
        self.assertEqual(result['type']['id'], project_type)
        self.assertEqual(len(result['type']['providers']), 1)
        self.assertEqual(result['type']['providers'][0]['id'], provider)
        # self.assertEqual(len(result['users']), 1)
        # self.assertEqual(result['users'][0]['id'], user)

        cmd = 'tc project show -u {base_url} -n {project}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        print(result)
        self.assertEqual(result['name'], project)
        self.assertEqual(result['type']['id'], project_type)
        self.assertEqual(len(result['type']['providers']), 1)
        self.assertEqual(result['type']['providers'][0]['id'], provider)
        # self.assertEqual(len(result['users']), 1)
        # self.assertEqual(result['users'][0]['id'], user)

        cmd = 'tc project list -u {base_url}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        print(result)
        self.assertEqual(len(result), 1)
        self.assertEqual(result[0]['name'], project)
        self.assertEqual(result[0]['type']['id'], project_type)
        self.assertEqual(len(result[0]['type']['providers']), 1)
        self.assertEqual(result[0]['type']['providers'][0]['id'], provider)
        # self.assertEqual(len(result[0]['users']), 1)
        # self.assertEqual(result[0]['users'][0]['id'], user)

        cmd = 'tc project delete -u {base_url} -n {project} -y'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        print(result)

        cmd = 'tc project list -u {base_url}'.format(**locals())
        result = self.cmd(cmd).get_output_in_json()
        print(result)
        self.assertEqual(len(result), 0)

        # cmd = 'tc project-type delete -u {base_url} -n {project_type} -y'.format(**locals())
        # result = self.cmd(cmd)

        # cmd = 'tc project-type list -u {base_url}'.format(**locals())
        # result = self.cmd(cmd).get_output_in_json()
        # self.assertEqual(len(result), 0)

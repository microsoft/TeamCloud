# coding=utf-8
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See License.txt in the project root for license information.
# --------------------------------------------------------------------------------------------

from knack.help_files import helps  # pylint: disable=unused-import

# ----------------
# TeamCloud
# ----------------

helps['tc'] = """
type: group
short-summary: Manage TeamCloud instances.
"""

helps['tc deploy'] = """
type: command
short-summary: Deploy a new TeamCloud instance.
examples:
  - name: Deploy a new TeamCloud instance.
    text: az tc deploy --name myawesomeapp
  - name: Deploy a TeamCloud instance to a specific pre-release.
    text: az tc deploy --name myawesomeapp --version v0.1.1
"""

helps['tc upgrade'] = """
type: command
short-summary: Upgrade a TeamCloud instance version.
examples:
  - name: Upgrade a TeamCloud instance to the latest release.
    text: az tc upgrade
  - name: Upgrade a TeamCloud instance to a specific pre-release.
    text: az tc upgrade --version v0.1.1
"""

helps['tc status'] = """
type: command
short-summary: Get the status of a long-running operation.
examples:
  - name: Get the status of a TeamCloud operation like creating a new provider.
    text: az tc status --tracking-id myTrackingIdGuid
  - name: Get the status of a Project operation like creating a new project user.
    text: az tc status --project myProjectId --tracking-id myTrackingIdGuid
"""

# ----------------
# TeamCloud Users
# ----------------

helps['tc user'] = """
type: group
short-summary: Manage TeamCloud (system) users.
"""

helps['tc user create'] = """
type: command
short-summary: Create a new TeamCloud user.
examples:
  - name: Create a new TeamCloud user with Admin role.
    text: az tc user create --name 'user@microsoft.com' --role Admin --tags tag=value
"""

helps['tc user delete'] = """
type: command
short-summary: Delete a TeamCloud user.
examples:
  - name: Delete a user by email address.
    text: az tc user delete --name 'user@microsoft.com'
  - name: Delete a user by id.
    text: az tc user delete --name userId
"""

helps['tc user list'] = """
type: command
short-summary: List all TeamCloud users.
examples:
  - name: List all users.
    text: az tc user list
  - name: List all users in table format.
    text: az tc user list -o table
"""

helps['tc user show'] = """
type: command
short-summary: Get a TeamCloud user.
examples:
  - name: Get a user by email address.
    text: az tc user show --name 'user@microsoft.com'
  - name: Get a user by id.
    text: az tc user show --name userId
"""

# ----------------
# TeamCloud Tags
# ----------------

helps['tc tag'] = """
type: group
short-summary: Manage TeamCloud tags.
"""

helps['tc tag create'] = """
type: command
short-summary: Create a new TeamCloud tag.
examples:
  - name: Create a new TeamCloud tag.
    text: az tc tag create --key myTag --value myTagValue
"""

helps['tc tag delete'] = """
type: command
short-summary: Delete a TeamCloud tag.
examples:
  - name: Delete a TeamCloud tag by key.
    text: az tc tag delete --key myTag
"""

helps['tc tag list'] = """
type: command
short-summary: List all TeamCloud tags.
examples:
  - name: List all TeamCloud tags in table format.
    text: az tc tag list -o table
"""

helps['tc tag show'] = """
type: command
short-summary: Get a TeamCloud tag.
examples:
  - name: Get a TeamCloud tag by key.
    text: az tc tag show --key myTag
"""

# ----------------
# Projects
# ----------------

helps['tc project'] = """
type: group
short-summary: Manage TeamCloud projects.
"""

helps['tc project create'] = """
type: command
short-summary: Create a new project.
examples:
  - name: Create a new project using the default project type.
    text: az tc project create --name MyProject1 --tags tag=value
  - name: Create a new project using a specific project type.
    text: az tc project create --name MyProject2 --project-type my.project.type --tags tag=value
"""

helps['tc project delete'] = """
type: command
short-summary: Delete a project.
examples:
  - name: Delete a project by name.
    text: az tc project delete --name MyProject1
  - name: Delete a project by id.
    text: az tc project delete --name myProjectId
"""

helps['tc project list'] = """
type: command
short-summary: List all projects.
examples:
  - name: List all projects.
    text: az tc project list
  - name: List all projects in table format.
    text: az tc project list -o table
"""

helps['tc project show'] = """
type: command
short-summary: Get a project.
examples:
  - name: Get a project by name.
    text: az tc project show --name MyProject1
  - name: Get a project by id.
    text: az tc project show --name myProjectId
"""

# ----------------
# Project Users
# ----------------

helps['tc project user'] = """
type: group
short-summary: Manage project users.
"""

helps['tc project user create'] = """
type: command
short-summary: Create a new project user.
examples:
  - name: Create a new project user with Owner role.
    text: az tc project user create --project myProjectId --name 'user@microsoft.com' --role Owner --tags tag=value
"""

helps['tc project user delete'] = """
type: command
short-summary: Delete a project user.
examples:
  - name: Delete a project user by email address.
    text: az tc project user delete --project myProjectId --name 'user@microsoft.com'
  - name: Delete a project user by id.
    text: az tc project user delete --project myProjectId --name userId
"""

helps['tc project user list'] = """
type: command
short-summary: List all project users.
examples:
  - name: List all project users.
    text: az tc project user list --project myProjectId
  - name: List all project users in table format.
    text: az tc project user list --project myProjectId -o table
"""

helps['tc project user show'] = """
type: command
short-summary: Get a project user.
examples:
  - name: Get a project user by email address.
    text: az tc project user show --project myProjectId --name 'user@microsoft.com'
  - name: Get a project user by id.
    text: az tc project user show --project myProjectId --name userId
"""

# ----------------
# Project Tags
# ----------------

helps['tc project tag'] = """
type: group
short-summary: Manage project tags.
"""

helps['tc project tag create'] = """
type: command
short-summary: Create a new project tag.
examples:
  - name: Create a new project tag.
    text: az tc project tag create --project myProjectId --key myTag --value myTagValue
"""

helps['tc project tag delete'] = """
type: command
short-summary: Delete a project tag.
examples:
  - name: Delete a project tag by key.
    text: az tc project tag delete --project myProjectId --key myTag
"""

helps['tc project tag list'] = """
type: command
short-summary: List all project tags.
examples:
  - name: List all project tags in table format.
    text: az tc project tag list --project myProjectId -o table
"""

helps['tc project tag show'] = """
type: command
short-summary: Get a project tag.
examples:
  - name: Get a project tag by key.
    text: az tc project tag show --project myProjectId --key myTag
"""

# ----------------
# Project Types
# ----------------

helps['tc project-type'] = """
type: group
short-summary: Manage project types.
"""

helps['tc project-type create'] = """
type: command
short-summary: Create a new project type.
examples:
  - name: Create a new default project type.
    text: |
      az tc project-type create \\
        --name my.project.type \\
        --location eastus \\
        --subscriptions subsciptionId1 subsciptionId2 subsciptionId3 \\
        --subscription-capacity 5 \\
        --resource-group-name-prefix TC_ \\
        --provider my.provider.id.one prop1=val1 prop2=val2 \\
        --provider my.provider.id.two prop3=val3 prop4=val4 depends_on=my.provider.id.one \\
        --default
"""

helps['tc project-type delete'] = """
type: command
short-summary: Delete a project type.
examples:
  - name: Delete a project type.
    text: az tc project-type delete --name my.project.type
"""

helps['tc project-type list'] = """
type: command
short-summary: List all project types.
examples:
  - name: List all project types.
    text: az tc project-type list
  - name: List all project types in table format.
    text: az tc project-type list -o table
"""

helps['tc project-type show'] = """
type: command
short-summary: Get a project type.
examples:
  - name: Get a project-type.
    text: az tc project-type show --name my.project.type
"""

# ----------------
# Providers
# ----------------

helps['tc provider'] = """
type: group
short-summary: Manage providers.
"""

helps['tc provider create'] = """
type: command
short-summary: Create a new provider.
examples:
  - name: Create a new provider.
    text: |
      az tc provider create \\
        --name azure.devtestlabs \\
        --url https://my-provider.azurewebsites.net \\
        --auth-code cmFuZG9tcmFuZG9tcmFuZG9tcmFuZG9tcmFuZG9tcmFuZG9tcmFuZA== \\
        --events azure.devtestlabs azure.appinsights
"""

helps['tc provider delete'] = """
type: command
short-summary: Delete a provider.
examples:
  - name: Delete a provider.
    text: az tc provider delete --name my.provider.id
"""

helps['tc provider list'] = """
type: command
short-summary: List all providers.
examples:
  - name: List all providers.
    text: az tc provider list
  - name: List all providers in table format.
    text: az tc provider list -o table
"""

helps['tc provider show'] = """
type: command
short-summary: Get a provider.
examples:
  - name: Get a provider.
    text: az tc provider show --name my.provider.id
"""

helps['tc provider deploy'] = """
type: command
short-summary: Deploy a provider.
examples:
  - name: Deploy a provider.
    text: az tc provider deploy --name azure.devtestlabs
  - name: Deploy a provider to a specific pre-release.
    text: az tc provider deploy --name azure.devtestlabs --version v0.1.1
"""

helps['tc provider upgrade'] = """
type: command
short-summary: Upgrade a provider version.
examples:
  - name: Upgrade provider to the latest version.
    text: az tc provider upgrade --name azure.devtestlabs
  - name: Upgrade provider to a specific pre-release.
    text: az tc provider upgrade --name azure.devtestlabs --version v0.1.1
"""

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

helps['tc update'] = """
type: command
short-summary: Update tc cli extension.
examples:
  - name: Update tc cli extension.
    text: az tc update
  - name: Update tc cli extension to the latest pre-release.
    text: az tc update --pre
"""

helps['tc info'] = """
type: command
short-summary: Get TeamCloud instance information.
examples:
  - name: Get TeamCloud instance information.
    text: az tc info --base-url url
"""

helps['tc deploy'] = """
type: command
short-summary: Deploy a new TeamCloud instance.
examples:
  - name: Deploy a new TeamCloud instance.
    text: az tc deploy --name myawesomeapp --location eastus
  - name: Deploy a TeamCloud instance to a specific pre-release.
    text: az tc deploy --name myawesomeapp --location eastus --version v0.1.1
"""

helps['tc upgrade'] = """
type: command
short-summary: Upgrade a TeamCloud instance version.
examples:
  - name: Upgrade a TeamCloud instance to the latest release.
    text: az tc upgrade --base-url url
  - name: Upgrade a TeamCloud instance to a specific pre-release.
    text: az tc upgrade --base-url url --version v0.1.1
"""

helps['tc status'] = """
type: command
short-summary: Get the status of a long-running operation.
examples:
  - name: Get the status of a TeamCloud operation like creating a new provider.
    text: az tc status --base-url url --tracking-id trackingId
  - name: Get the status of a Project operation like creating a new project user.
    text: az tc status --base-url url --project project --tracking-id trackingId
"""

# ----------------
# TeamCloud Apps
# ----------------

helps['tc app'] = """
type: group
short-summary: Manage client applications.
"""

helps['tc app deploy'] = """
type: command
short-summary: Deploy a new client applications.
examples:
  - name: Deploy a new web app for a instance.
    text: az tc app deploy --base-url url --client-id clientId --type Web
  - name: Deploy new pre-release web app for a instance.
    text: az tc app deploy --base-url url --client-id clientId --type Web --pre
"""

helps['tc app upgrade'] = """
type: command
short-summary: Upgrade a client application.
examples:
  - name: Upgrade a web app to the latest version.
    text: az tc app upgrade --base-url url --client-id clientId --type Web
  - name: Upgrade a web app to the latest pre-release version.
    text: az tc app upgrade --base-url url --client-id clientId --type Web --pre
"""

# ----------------
# TeamCloud Users
# ----------------

helps['tc user'] = """
type: group
short-summary: Manage (system) users.
"""

helps['tc user create'] = """
type: command
short-summary: Create a new user.
examples:
  - name: Create a new user with Admin role.
    text: az tc user create --base-url url --name user --role Admin --properties prop=value
"""

helps['tc user delete'] = """
type: command
short-summary: Delete a user.
examples:
  - name: Delete a user by email address.
    text: az tc user delete --base-url url --name user
  - name: Delete a user by id.
    text: az tc user delete --base-url url --name userId
"""

helps['tc user list'] = """
type: command
short-summary: List all users.
examples:
  - name: List all users.
    text: az tc user list --base-url url
  - name: List all users in table format.
    text: az tc user list --base-url url -o table
"""

helps['tc user show'] = """
type: command
short-summary: Get a user.
examples:
  - name: Get a user by email address.
    text: az tc user show --base-url url --name user
  - name: Get a user by id.
    text: az tc user show --base-url url --name userId
"""

helps['tc user update'] = """
type: command
short-summary: Update a user.
examples:
  - name: Update a user's role.
    text: az tc user update --base-url url --name user --role Creator
  - name: Add a property to a user.
    text: az tc user update --base-url url --name user --properties prop=value
  - name: Add a property to a user using generic set.
    text: az tc user update --base-url url --name user --set properties.prop=value
"""

# ----------------
# TeamCloud Tags
# ----------------

helps['tc tag'] = """
type: group
short-summary: Manage (system) tags.
"""

helps['tc tag create'] = """
type: command
short-summary: Create a new tag.
examples:
  - name: Create a new tag.
    text: az tc tag create --base-url url --key key --value value
"""

helps['tc tag delete'] = """
type: command
short-summary: Delete a tag.
examples:
  - name: Delete a tag by key.
    text: az tc tag delete --base-url url --key key
"""

helps['tc tag list'] = """
type: command
short-summary: List all tags.
examples:
  - name: List all tags in table format.
    text: az tc tag list --base-url url -o table
"""

helps['tc tag show'] = """
type: command
short-summary: Get a tag.
examples:
  - name: Get a tag by key.
    text: az tc tag show --base-url url --key key
"""

# ----------------
# Projects
# ----------------

helps['tc project'] = """
type: group
short-summary: Manage projects.
"""

helps['tc project create'] = """
type: command
short-summary: Create a new project.
examples:
  - name: Create a new project using the default project type.
    text: az tc project create --base-url url --name project --tags tag=value --properties prop=value
  - name: Create a new project using a specific project type.
    text: az tc project create --base-url url --name project --project-type type --tags tag=value --properties prop=value
"""

helps['tc project delete'] = """
type: command
short-summary: Delete a project.
examples:
  - name: Delete a project by name or id.
    text: az tc project delete --base-url url --name project
"""

helps['tc project list'] = """
type: command
short-summary: List all projects.
examples:
  - name: List all projects.
    text: az tc project list --base-url url
  - name: List all projects in table format.
    text: az tc project list --base-url url -o table
"""

helps['tc project show'] = """
type: command
short-summary: Get a project.
examples:
  - name: Get a project by name or id.
    text: az tc project show --base-url url --name project
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
    text: az tc project user create --base-url urle --project project --name user --role Owner --properties prop=value
"""

helps['tc project user delete'] = """
type: command
short-summary: Delete a project user.
examples:
  - name: Delete a project user by email address or id.
    text: az tc project user delete --base-url url --project project --name user
"""

helps['tc project user list'] = """
type: command
short-summary: List all project users.
examples:
  - name: List all project users.
    text: az tc project user list --base-url url --project project
  - name: List all project users in table format.
    text: az tc project user list --base-url url --project project -o table
"""

helps['tc project user show'] = """
type: command
short-summary: Get a project user.
examples:
  - name: Get a project user by email address or id.
    text: az tc project user show --base-url url --project project --name user
"""

helps['tc project user update'] = """
type: command
short-summary: Update a project user.
examples:
  - name: Update a user's role.
    text: az tc project user update --base-url url --project project --name user --role Owner
  - name: Add a property to a user.
    text: az tc project user update --base-url url --project project --name user --properties prop=value
  - name: Add a property to a user using generic set.
    text: az tc project user update --base-url url --project project --name user --set properties.prop=value
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
    text: az tc project tag create --base-url url --project project --key key --value value
"""

helps['tc project tag delete'] = """
type: command
short-summary: Delete a project tag.
examples:
  - name: Delete a project tag by key.
    text: az tc project tag delete --base-url url --project project --key key
"""

helps['tc project tag list'] = """
type: command
short-summary: List all project tags.
examples:
  - name: List all project tags in table format.
    text: az tc project tag list --base-url url --project project -o table
"""

helps['tc project tag show'] = """
type: command
short-summary: Get a project tag.
examples:
  - name: Get a project tag by key.
    text: az tc project tag show --base-url url --project project --key key
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
        --base-url url \\
        --name type \\
        --location eastus \\
        --subscriptions subscriptionId1 subscriptionId2 subscriptionId3 \\
        --subscription-capacity 5 \\
        --resource-group-name-prefix TC_ \\
        --provider provider.one prop1=val1 prop2=val2 \\
        --provider provider.two prop3=val3 prop4=val4 depends_on=provider.one \\
        --default
"""

helps['tc project-type delete'] = """
type: command
short-summary: Delete a project type.
examples:
  - name: Delete a project type.
    text: az tc project-type delete --base-url url --name type
"""

helps['tc project-type list'] = """
type: command
short-summary: List all project types.
examples:
  - name: List all project types.
    text: az tc project-type list --base-url url
  - name: List all project types in table format.
    text: az tc project-type list --base-url url -o table
"""

helps['tc project-type show'] = """
type: command
short-summary: Get a project type.
examples:
  - name: Get a project-type.
    text: az tc project-type show --base-url url --name type
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
        --base-url url \\
        --name azure.devtestlabs \\
        --url https://my-provider.azurewebsites.net/api/command \\
        --auth-code cmFuZG9tcmFuZG9tcmFuZG9tcmFuZG9tcmFuZG9tcmFuZG9tcmFuZA==
"""

helps['tc provider delete'] = """
type: command
short-summary: Delete a provider.
examples:
  - name: Delete a provider.
    text: az tc provider delete --base-url url --name provider
"""

helps['tc provider list'] = """
type: command
short-summary: List all providers.
examples:
  - name: List all providers.
    text: az tc provider list --base-url url
  - name: List all providers in table format.
    text: az tc provider list --base-url url -o table
"""

helps['tc provider list-available'] = """
type: command
short-summary: List available providers.
examples:
  - name: List available providers.
    text: az tc provider list-available
  - name: List details on a particular provider.
    text: az tc provider list-available --show-details --query github
"""

helps['tc provider show'] = """
type: command
short-summary: Get a provider.
examples:
  - name: Get a provider.
    text: az tc provider show --base-url url --name provider
"""

helps['tc provider deploy'] = """
type: command
short-summary: Deploy a provider.
examples:
  - name: Deploy a provider.
    text: az tc provider deploy --base-url url --location eastus --name azure.devtestlabs
  - name: Deploy a provider to a specific pre-release.
    text: az tc provider deploy --base-url url --location eastus --name azure.devtestlabs --version v0.1.1
"""

helps['tc provider upgrade'] = """
type: command
short-summary: Upgrade a provider version.
examples:
  - name: Upgrade provider to the latest version.
    text: az tc provider upgrade --base-url url --name azure.devtestlabs
  - name: Upgrade provider to a specific pre-release.
    text: az tc provider upgrade --base-url url --name azure.devtestlabs --version v0.1.1
"""

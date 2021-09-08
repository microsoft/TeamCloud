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

helps['tc deploy'] = """
type: command
short-summary: Deploy a new TeamCloud instance.
examples:
  - name: Deploy a new TeamCloud instance.
    text: az tc deploy --name myawesomeapp --location eastus --client-id myWebClientId
  - name: Deploy a TeamCloud instance to a specific pre-release.
    text: az tc deploy --name myawesomeapp --location eastus --client-id myWebClientId --version v0.1.1
"""

# ----------------
# TeamCloud Orgs
# ----------------

helps['tc org'] = """
type: group
short-summary: Manage organizations.
"""

helps['tc org create'] = """
type: command
short-summary: Create a new organization.
examples:
  - name: Create a new organization with Admin role.
    text: az tc org create --url url --name MyOrg --location eastus
"""

helps['tc org delete'] = """
type: command
short-summary: Delete a organization.
examples:
  - name: Delete a organization by slug.
    text: az tc org delete --url url --name myorg
  - name: Delete a organization by id.
    text: az tc org delete --url url --name orgId
"""

helps['tc org list'] = """
type: command
short-summary: List all organizations.
examples:
  - name: List all organizations.
    text: az tc org list --url url
  - name: List all organizations in table format.
    text: az tc org list --url url -o table
"""

helps['tc org show'] = """
type: command
short-summary: Get a organization.
examples:
  - name: Get a organization by slug.
    text: az tc org show --url url --name myorg
  - name: Get a organization by id.
    text: az tc org show --url url --name orgId
"""

# ----------------
# Deployment Scopes
# ----------------

helps['tc scope'] = """
type: group
short-summary: Manage deployment scopes.
"""

helps['tc scope create'] = """
type: command
short-summary: Create a new deployment scope.
examples:
  - name: Create a new deployment scope.
    text: az tc scope create --url url --org org --name Sandbox
"""

helps['tc scope delete'] = """
type: command
short-summary: Delete a deployment scope.
examples:
  - name: Delete a deployment scope by name or id.
    text: az tc scope delete --url url --org org --name Sandbox
"""

helps['tc scope list'] = """
type: command
short-summary: List all deployment scopes.
examples:
  - name: List all deployment scopes.
    text: az tc scope list --url url --org org
  - name: List all deployment scopes in table format.
    text: az tc scope list --url url --org org -o table
"""

helps['tc scope show'] = """
type: command
short-summary: Get a deployment scope.
examples:
  - name: Get a deployment scope by name or id.
    text: az tc scope show --url url --org org --name sandbox
"""

# ----------------
# Project Templates
# ----------------

helps['tc template'] = """
type: group
short-summary: Manage project templates.
"""

helps['tc template create'] = """
type: command
short-summary: Create a new project template.
examples:
  - name: Create a new default project template.
    text: |
      az tc template create \\
        --url url \\
        --org org \\
        --name myTemplate \\
        --repo-url https://github.com/microsoft/TeamCloud-Project-Sample \\
        --repo-version main
"""

helps['tc template delete'] = """
type: command
short-summary: Delete a project template.
examples:
  - name: Delete a project template.
    text: az tc template delete --url url --org org --name myTemplate
"""

helps['tc template list'] = """
type: command
short-summary: List all project templates.
examples:
  - name: List all project templates.
    text: az tc template list --url url --org org
  - name: List all project templates in table format.
    text: az tc template list --url url --org org -o table
"""

helps['tc template show'] = """
type: command
short-summary: Get a project template.
examples:
  - name: Get a project-type.
    text: az tc template show --url url --org org --name myTemplate
"""

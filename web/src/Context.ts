// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Component, ComponentDefinition, ComponentTask, ComponentTaskDefinition, ComponentTemplate, DeploymentScope, DeploymentScopeDefinition, Organization, OrganizationDefinition, Project, ProjectDefinition, ProjectTemplate, ProjectTemplateDefinition, User, UserDefinition } from 'teamcloud'
import { GraphUser, ManagementGroup, Member, ProjectMember, Subscription } from './model';

export const GraphUserContext = React.createContext({
    graphUser: undefined as GraphUser | undefined,
});

export const AzureManagementContext = React.createContext({
    subscriptions: undefined as Subscription[] | undefined,
    managementGroups: undefined as ManagementGroup[] | undefined,
});

export const OrgsContext = React.createContext({
    orgs: undefined as Organization[] | undefined,
    createOrg: (def: { orgDef: OrganizationDefinition, scopeDef?: DeploymentScopeDefinition, templateDef?: ProjectTemplateDefinition }) => Promise.resolve(),
});

export const OrgContext = React.createContext({
    org: undefined as Organization | undefined,
    user: undefined as User | undefined,
    members: undefined as Member[] | undefined,
    scopes: undefined as DeploymentScope[] | undefined,
    templates: undefined as ProjectTemplate[] | undefined,
    projects: undefined as Project[] | undefined,
    addUsers: (users: UserDefinition[]) => Promise.resolve(),
    removeUsers: (users: User[]) => Promise.resolve(),
    createProject: (projectDef: ProjectDefinition) => Promise.resolve(),
    createDeploymentScope: (scope: DeploymentScopeDefinition) => Promise.resolve(),
    createProjectTemplate: (template: ProjectTemplateDefinition) => Promise.resolve(),
});

export const ProjectContext = React.createContext({
    project: undefined as Project | undefined,
    members: undefined as ProjectMember[] | undefined,
    components: undefined as Component[] | undefined,
    component: undefined as Component | undefined,
    templates: undefined as ComponentTemplate[] | undefined,
    componentTasks: undefined as ComponentTask[] | undefined,
    componentTask: undefined as ComponentTask | undefined,
    addUsers: (users: UserDefinition[]) => Promise.resolve(),
    removeUsers: (users: User[]) => Promise.resolve(),
    createComponent: (componentDef: ComponentDefinition) => Promise.resolve(),
    createComponentTask: (componentTaskDef: ComponentTaskDefinition) => Promise.resolve(),
});

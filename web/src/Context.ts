// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Component, ComponentTask, ComponentTemplate, DeploymentScope, DeploymentScopeDefinition, Organization, Project, ProjectTemplate, ProjectTemplateDefinition, User, UserDefinition } from 'teamcloud'
import { GraphUser, ManagementGroup, Member, ProjectMember, Subscription } from './model';

export const GraphUserContext = React.createContext({
    graphUser: undefined as GraphUser | undefined,
    setGraphUser: (graphUser?: GraphUser) => { },
    subscriptions: undefined as Subscription[] | undefined,
    managementGroups: undefined as ManagementGroup[] | undefined,
});

export const OrgContext = React.createContext({
    org: undefined as Organization | undefined,
    orgs: undefined as Organization[] | undefined,
    user: undefined as User | undefined,
    members: undefined as Member[] | undefined,
    scopes: undefined as DeploymentScope[] | undefined,
    templates: undefined as ProjectTemplate[] | undefined,
    projects: undefined as Project[] | undefined,
    onOrgSelected: (org?: Organization) => { },
    onProjectSelected: (project?: Project) => { },
    onAddUsers: (users: UserDefinition[]) => Promise.resolve(),
    onRemoveUsers: (users: User[]) => Promise.resolve(),
    onCreateDeploymentScope: (scope: DeploymentScopeDefinition, org?: Organization) => Promise.resolve(),
    onCreateProjectTemplate: (template: ProjectTemplateDefinition, org?: Organization) => Promise.resolve(),
});

export const ProjectContext = React.createContext({
    user: undefined as User | undefined,
    project: undefined as Project | undefined,
    members: undefined as ProjectMember[] | undefined,
    components: undefined as Component[] | undefined,
    component: undefined as Component | undefined,
    templates: undefined as ComponentTemplate[] | undefined,
    componentTasks: undefined as ComponentTask[] | undefined,
    componentTask: undefined as ComponentTask | undefined,
    onComponentSelected: (component?: Component) => { },
    onComponentTaskSelected: (componentTask?: ComponentTask) => { },
    onAddUsers: (users: UserDefinition[]) => Promise.resolve(),
    onRemoveUsers: (users: User[]) => Promise.resolve(),
});

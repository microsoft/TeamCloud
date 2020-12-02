// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Route, Switch } from 'react-router-dom';
import { Organization, Project, User } from 'teamcloud';
import { Error404, NewOrgView, NewProjectView, ProjectView, ProjectsView, OrgSettingsView, ProjectSettingsView } from '.';
import { NewComponentView } from './NewComponentView';

export interface IContentViewProps {
    org?: Organization;
    user?: User;
    project?: Project;
    projects?: Project[];
    onOrgSelected: (org?: Organization) => void;
    onProjectSelected: (project: Project) => void;
}

export const ContentView: React.FC<IContentViewProps> = (props: IContentViewProps) => {

    return (
        <Switch>
            <Route exact path='/'>
                <></>
            </Route>
            <Route exact path='/orgs/new'>
                <NewOrgView onOrgSelected={props.onOrgSelected} />
            </Route>
            <Route exact path='/orgs/:orgId'>
                <ProjectsView {...{ org: props.org, projects: props.projects, onProjectSelected: props.onProjectSelected }} />
            </Route>
            <Route exact path='/orgs/:orgId/projects/new'>
                <NewProjectView {...{ org: props.org, onProjectSelected: props.onProjectSelected }} />
            </Route>
            <Route exact path={['/orgs/:orgId/settings', '/orgs/:orgId/settings/:settingId', '/orgs/:orgId/settings/:settingId/new']}>
                <OrgSettingsView {...{ org: props.org, }} />
            </Route>
            <Route exact path={['/orgs/:orgId/projects/:projectId/settings', '/orgs/:orgId/projects/:projectId/settings/:settingId']}>
                <ProjectSettingsView {...{ project: props.project }} />
            </Route>
            <Route exact path={['/orgs/:orgId/projects/:projectId', '/orgs/:orgId/projects/:projectId/:navId']}>
                <ProjectView {...{ user: props.user, project: props.project }} />
            </Route>
            <Route exact path='/orgs/:orgId/projects/:projectId/components/new'>
                <NewComponentView {...{ org: props.org, project: props.project }} />
            </Route>
            <Route path='*'>
                <Error404 />
            </Route>
        </Switch>
    );
}

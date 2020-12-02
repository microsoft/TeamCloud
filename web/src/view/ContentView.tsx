// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Route, Switch } from 'react-router-dom';
import { Error404, NewComponentView, NewOrgView, NewProjectView, ProjectView, ProjectsView, OrgSettingsView, ProjectSettingsView } from '.';

export const ContentView: React.FC = () => (
    <Switch>
        <Route exact path='/'>
            <></>
        </Route>
        <Route exact path='/orgs/new'>
            <NewOrgView />
        </Route>
        <Route exact path='/orgs/:orgId'>
            <ProjectsView />
        </Route>
        <Route exact path='/orgs/:orgId/projects/new'>
            <NewProjectView />
        </Route>
        <Route exact path={[
            '/orgs/:orgId/settings',
            '/orgs/:orgId/settings/:settingId',
            '/orgs/:orgId/settings/:settingId/new'
        ]}>
            <OrgSettingsView />
        </Route>
        <Route exact path={[
            '/orgs/:orgId/projects/:projectId/settings',
            '/orgs/:orgId/projects/:projectId/settings/:settingId'
        ]}>
            <ProjectSettingsView />
        </Route>
        <Route exact path={[
            '/orgs/:orgId/projects/:projectId',
            '/orgs/:orgId/projects/:projectId/:navId'
        ]}>
            <ProjectView />
        </Route>
        <Route exact path='/orgs/:orgId/projects/:projectId/components/new'>
            <NewComponentView />
        </Route>
        <Route path='*'>
            <Error404 />
        </Route>
    </Switch>
);

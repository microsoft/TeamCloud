// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Route, Switch } from 'react-router-dom';
import { Error404, NewOrgView, NewProjectView, ProjectView, ProjectsView, OrgSettingsView } from '.';

export const ContentRouter: React.FC = () => (
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
            '/orgs/:orgId/projects/:projectId',
            '/orgs/:orgId/projects/:projectId/settings',
            '/orgs/:orgId/projects/:projectId/settings/:settingId',
            '/orgs/:orgId/projects/:projectId/:navId',
            '/orgs/:orgId/projects/:projectId/:navId/new',
            '/orgs/:orgId/projects/:projectId/:navId/:itemId',
            '/orgs/:orgId/projects/:projectId/:navId/:itemId/tasks/:subitemId',
        ]}>
            <ProjectView />
        </Route>
        <Route path='*'>
            <Error404 />
        </Route>
    </Switch>
);

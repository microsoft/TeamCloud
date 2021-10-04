// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Route, Switch } from 'react-router-dom';
import { OrgSettingsNav, ProjectNav, ProjectSettingsNav, RootNav } from '../components';

export const NavRouter: React.FC = () => (
    <div style={{ height:"calc(100vh - 70px)", overflow:"scroll", overflowX:"hidden", overflowY: "auto" }}>
    <Switch>
        <Route exact path={[
            '/',
            '/orgs/:orgId',
            '/orgs/:orgId/projects/new'
        ]}>
            <RootNav {...{}} />
        </Route>
        <Route exact path={[
            '/orgs/:orgId/settings',
            '/orgs/:orgId/settings/:settingId',
            '/orgs/:orgId/settings/:settingId/new'
        ]}>
            <OrgSettingsNav {...{}} />
        </Route>
        <Route exact path={[
            '/orgs/:orgId/projects/:projectId/settings',
            '/orgs/:orgId/projects/:projectId/settings/:settingId',
            '/orgs/:orgId/projects/:projectId/settings/:settingId/new',
            '/orgs/:orgId/projects/:projectId/settings/:settingId/:itemId',
        ]}>
            <ProjectSettingsNav {...{}} />
        </Route>
        <Route exact path={[
            '/orgs/:orgId/projects/:projectId',
            '/orgs/:orgId/projects/:projectId/:navId',
            '/orgs/:orgId/projects/:projectId/:navId/new',
            '/orgs/:orgId/projects/:projectId/:navId/:itemId',
            '/orgs/:orgId/projects/:projectId/:navId/:itemId/tasks/:subitemId',
        ]}>
            <ProjectNav {...{}} />
        </Route>
    </Switch>
    </div>
);

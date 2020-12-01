// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Redirect, Route, Switch } from 'react-router-dom';
import { OrgSettingsNav, ProjectNav, ProjectSettingsNav, RootNav } from '../components';

export interface INavViewProps { }

export const NavView: React.FC<INavViewProps> = (props: INavViewProps) => {

    return (
        <Switch>
            <Redirect exact from='/orgs' to='/' />
            <Redirect exact from='/orgs/:orgId/projects' to='/orgs/:orgId' />
            <Redirect exact from='/orgs/:orgId/settings/overview' to='/orgs/:orgId/settings' />
            <Redirect exact from='/orgs/:orgId/projects/:projectId/overview' to='/orgs/:orgId/projects/:projectId' />
            <Redirect exact from='/orgs/:orgId/projects/:projectId/settings/overview' to='/orgs/:orgId/projects/:projectId/settings' />
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
                '/orgs/:orgId/projects/:projectId/settings/:settingId'
            ]}>
                <ProjectSettingsNav {...{}} />
            </Route>
            <Route exact path={[
                '/orgs/:orgId/projects/:projectId',
                '/orgs/:orgId/projects/:projectId/:navId',
                '/orgs/:orgId/projects/:projectId/:navId/new'
            ]}>
                <ProjectNav {...{}} />
            </Route>
        </Switch>
    );
}

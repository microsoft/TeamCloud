// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Redirect, Route, Switch } from 'react-router-dom';
import { Organization, Project, User } from 'teamcloud';
import { OrgSettingsNav, ProjectNav, ProjectSettingsNav, RootNav } from '../components';

export interface INavViewProps {
    org?: Organization;
    orgs?: Organization[];
    user?: User;
    project?: Project;
    projects?: Project[];
    onOrgSelected: (org?: Organization) => void;
    onProjectSelected: (project?: Project) => void;
}

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
                <RootNav {...{ org: props.org, orgs: props.orgs, onOrgSelected: props.onOrgSelected }} />
            </Route>
            <Route exact path={[
                '/orgs/:orgId/settings',
                '/orgs/:orgId/settings/:settingId',
                '/orgs/:orgId/settings/:settingId/new'
            ]}>
                <OrgSettingsNav {...{ org: props.org, orgs: props.orgs, onOrgSelected: props.onOrgSelected }} />
            </Route>
            <Route exact path={[
                '/orgs/:orgId/projects/:projectId/settings',
                '/orgs/:orgId/projects/:projectId/settings/:settingId'
            ]}>
                <ProjectSettingsNav {...{ ...props }} />
            </Route>
            <Route exact path={[
                '/orgs/:orgId/projects/:projectId',
                '/orgs/:orgId/projects/:projectId/:navId',
                '/orgs/:orgId/projects/:projectId/:navId/new'
            ]}>
                <ProjectNav {...{ ...props }} />
            </Route>
        </Switch>
    );
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useContext, useEffect } from 'react';
import { Redirect, Route, Switch, useParams } from 'react-router-dom';
import { OrgContext } from './Context';

export interface IStateRouterProps { }

export const StateRouter: React.FC<IStateRouterProps> = (props) => {

    return (
        <Switch>
            <Redirect exact from='/orgs' to='/' />
            <Redirect exact from='/orgs/:orgId/projects' to='/orgs/:orgId' />
            <Redirect exact from='/orgs/:orgId/settings/overview' to='/orgs/:orgId/settings' />
            <Redirect exact from='/orgs/:orgId/projects/:projectId/overview' to='/orgs/:orgId/projects/:projectId' />
            <Redirect exact from='/orgs/:orgId/projects/:projectId/settings/overview' to='/orgs/:orgId/projects/:projectId/settings' />
            <Route exact path={[
                '/',
                '/orgs/new',
                '/orgs/:orgId',
                '/orgs/:orgId/settings',
                '/orgs/:orgId/settings/:settingId',
                '/orgs/:orgId/settings/:settingId/new',
                '/orgs/:orgId/projects/new',
                '/orgs/:orgId/projects/:projectId',
                '/orgs/:orgId/projects/:projectId/settings',
                '/orgs/:orgId/projects/:projectId/settings/:settingId',
                '/orgs/:orgId/projects/:projectId/:navId',
                '/orgs/:orgId/projects/:projectId/:navId/new'
            ]}>
                <StateRouterContainer {...{}}>
                    {props.children}
                </StateRouterContainer>
            </Route>
        </Switch>
    );
}

export interface IStateRouterContainerProps { }

export const StateRouterContainer: React.FC<IStateRouterContainerProps> = (props) => {

    const { orgId, projectId, navId, settingId } = useParams() as { orgId: string, projectId: string, navId: string, settingId: string };

    const { org, orgs, onOrgSelected, project, projects, onProjectSelected } = useContext(OrgContext);

    useEffect(() => {
        if (orgId) {
            if (org && (org.id.toLowerCase() === orgId.toLowerCase() || org.slug.toLowerCase() === orgId.toLowerCase())) {
                return;
            } else if (orgs) {
                const find = orgs.find(o => o.id.toLowerCase() === orgId.toLowerCase() || o.slug.toLowerCase() === orgId.toLowerCase());
                if (find) {
                    console.log(`setOrg (${orgId})`);
                    onOrgSelected(find);
                }
            }
        }
    }, [orgId, org, orgs]);

    useEffect(() => {
        if (projectId) {
            if (project && (project.id.toLowerCase() === projectId.toLowerCase() || project.slug.toLowerCase() === projectId.toLowerCase())) {
                return;
            } else if (projects) {
                const find = projects.find(p => p.id.toLowerCase() === projectId.toLowerCase() || p.slug.toLowerCase() === projectId.toLowerCase());
                if (find) {
                    console.log(`setProject (${projectId})`);
                    onProjectSelected(find);
                }
            }
        }
    }, [projectId, project, projects]);

    return <>{props.children}</>;
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Route, Switch } from 'react-router-dom';
import { HeaderBar } from '../components';
import { GraphUser } from '../model';

export interface IHeaderViewProps {
    graphUser?: GraphUser;
}

export const HeaderView: React.FC<IHeaderViewProps> = (props) => {

    return (
        <Switch>
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
                <HeaderBar {...{ graphUser: props.graphUser }} />
            </Route>
        </Switch>
    );
}

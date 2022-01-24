// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect } from 'react';
import { InteractionType } from '@azure/msal-browser';
import { AuthenticatedTemplate, MsalAuthenticationResult, useMsalAuthentication } from '@azure/msal-react';
import { getTheme, Stack } from '@fluentui/react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { HeaderBar } from '../components';
import { auth } from '../API';
import { ContentRouter, NavRouter } from '.';


export interface IRootViewProps { }

export const RootView: React.FC<IRootViewProps> = (props) => {

    const theme = getTheme();

    const leftStackStyles = {
        root: {
            width: '260px',
            paddingTop: '20px',
            paddingBottom: '10px',
            borderRight: `${theme.palette.neutralLight} solid 1px`
        }
    };

    const rightStackStyles = {
        root: {
            backgroundColor: theme.palette.neutralLighterAlt
        }
    };

    const authResult: MsalAuthenticationResult = useMsalAuthentication(InteractionType.Redirect, { scopes: auth.getScopes() });

    useEffect(() => {
        if (authResult.error) {
            console.log('logging in...')
            authResult.login(InteractionType.Redirect, { scopes: auth.getScopes() });
        }
    }, [authResult]);


    return (
        <Routes>
            <Route path='/orgs' element={<Navigate to='/' replace />} />
            <Route path='/orgs/:orgId/projects' element={<Navigate replace to='/orgs/:orgId' />} />
            <Route path='/orgs/:orgId/settings/overview' element={<Navigate replace to='/orgs/:orgId/settings' />} />
            <Route path='/orgs/:orgId/projects/:projectId/overview' element={<Navigate replace to='/orgs/:orgId/projects/:projectId' />} />
            <Route path='/orgs/:orgId/projects/:projectId/settings/overview' element={<Navigate replace to='/orgs/:orgId/projects/:projectId/settings' />} />
            <Route path='/orgs/:orgId/projects/:projectId/components/:itemId/tasks' element={<Navigate replace to='/orgs/:orgId/projects/:projectId/components/:itemId' />} />
            <Route path='/*' element={
                <Stack verticalFill style={{ height: "100vh" }}>
                    <HeaderBar {...{}} />
                    <AuthenticatedTemplate>
                        <Stack horizontal disableShrink verticalFill verticalAlign='stretch'>
                            <Stack.Item styles={leftStackStyles}>
                                <NavRouter {...{}} />
                            </Stack.Item>
                            <Stack.Item grow styles={rightStackStyles}>
                                <ContentRouter {...{}} />
                            </Stack.Item>
                        </Stack>
                    </AuthenticatedTemplate>
                </Stack>
            } />
            {/* <Route exact path={[
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
                '/orgs/:orgId/projects/:projectId/settings/:settingId/new',
                '/orgs/:orgId/projects/:projectId/settings/:settingId/:itemId',
                '/orgs/:orgId/projects/:projectId/:navId',
                '/orgs/:orgId/projects/:projectId/:navId/new',
                '/orgs/:orgId/projects/:projectId/:navId/:itemId',
                '/orgs/:orgId/projects/:projectId/:navId/:itemId/tasks/:subitemId',
            ]}>
                <Stack verticalFill style={{ height:"100vh" }}>
                    <HeaderBar />
                    <AuthenticatedTemplate>
                        <Stack horizontal disableShrink verticalFill verticalAlign='stretch'>
                            <Stack.Item styles={leftStackStyles}>
                                <NavRouter {...{}} />
                            </Stack.Item>
                            <Stack.Item grow styles={rightStackStyles}>
                                <ContentRouter {...{}} />
                            </Stack.Item>
                        </Stack>
                    </AuthenticatedTemplate>
                </Stack>
            </Route> */}
        </Routes>
    );
}

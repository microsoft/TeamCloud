// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { getTheme, Stack } from '@fluentui/react';
import { InteractionType } from '@azure/msal-browser';
import { AuthenticatedTemplate, UnauthenticatedTemplate, MsalAuthenticationResult, useMsalAuthentication, useIsAuthenticated } from '@azure/msal-react';
import { Error403, ContentView, NavView, HeaderView } from './view';
import { auth } from './API';
import { GraphUser } from './model';
import { getMe } from './MSGraph';

interface IAppProps { }

export const App: React.FC<IAppProps> = () => {

    const isAuthenticated = useIsAuthenticated();

    const authResult: MsalAuthenticationResult = useMsalAuthentication(InteractionType.Redirect, { scopes: auth.getScopes() });

    useEffect(() => {
        if (authResult.error) {
            console.log('logging in...')
            authResult.login(InteractionType.Redirect, { scopes: auth.getScopes() });
        }
    }, [authResult]);

    const [graphUser, setGraphUser] = useState<GraphUser>();

    useEffect(() => {
        if (isAuthenticated && graphUser === undefined) {
            const _setGraphUser = async () => {
                const result = await getMe();
                setGraphUser(result);
            };
            _setGraphUser();
        }
    }, [isAuthenticated, graphUser]);

    const theme = getTheme();

    return (
        <Stack verticalFill>
            <BrowserRouter>
                <HeaderView graphUser={graphUser} />
                <UnauthenticatedTemplate>
                    <Error403 error={authResult.result ?? 'Unauthenticated'} />
                </UnauthenticatedTemplate>
                <AuthenticatedTemplate>
                    <Stack horizontal disableShrink verticalFill verticalAlign='stretch'>
                        <Stack.Item styles={{ root: { width: '260px', paddingTop: '20px', paddingBottom: '10px', borderRight: `${theme.palette.neutralLight} solid 1px` } }}>
                            <NavView />
                        </Stack.Item>
                        <Stack.Item grow styles={{ root: { backgroundColor: theme.palette.neutralLighterAlt } }}>
                            <ContentView />
                        </Stack.Item>
                    </Stack>
                </AuthenticatedTemplate>
            </BrowserRouter>
        </Stack>
    );
}

export default App;

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect } from 'react';
import { InteractionType } from '@azure/msal-browser';
import { AuthenticatedTemplate, MsalAuthenticationResult, useMsalAuthentication } from '@azure/msal-react';
import { getTheme, Stack } from '@fluentui/react';
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
    );
}

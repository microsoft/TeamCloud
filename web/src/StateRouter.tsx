// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect } from 'react';
import { InteractionType } from '@azure/msal-browser';
import { MsalAuthenticationResult, useMsalAuthentication } from '@azure/msal-react';
import { AzureManagementProvider, GraphUserProvider, OrgsProvider, OrgProvider, ProjectProvider } from './providers'
import { auth } from './API';

export interface IStateRouterProps { }

export const StateRouter: React.FC<IStateRouterProps> = (props) => {

    const authResult: MsalAuthenticationResult = useMsalAuthentication(InteractionType.Redirect, { scopes: auth.getScopes() });

    useEffect(() => {
        if (authResult.error) {
            console.log('logging in...')
            authResult.login(InteractionType.Redirect, { scopes: auth.getScopes() });
        }
    }, [authResult]);

    return (
        <GraphUserProvider>
            <AzureManagementProvider>
                <OrgsProvider>
                    <OrgProvider>
                        <ProjectProvider>
                            {props.children}
                        </ProjectProvider>
                    </OrgProvider>
                </OrgsProvider>
            </AzureManagementProvider>
        </GraphUserProvider>
    );
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Configuration, InteractionRequiredAuthError, PublicClientApplication } from '@azure/msal-browser';
import { AccessToken, TokenCredential } from '@azure/core-auth'
import { AuthenticationProvider, AuthenticationProviderOptions } from "@microsoft/microsoft-graph-client";

export class Auth implements TokenCredential, AuthenticationProvider {

    _getClientId = () => {
        if (process.env.NODE_ENV !== 'production') {
            if (!process.env.REACT_APP_MSAL_CLIENT_ID) throw new Error('Must set env variable $REACT_APP_MSAL_CLIENT_ID');
            return process.env.REACT_APP_MSAL_CLIENT_ID;
        }
        
        return '__REACT_APP_MSAL_CLIENT_ID__';
    };

    _getAuthority = () => {
        if (process.env.NODE_ENV !== 'production') {
            if (!process.env.REACT_APP_MSAL_TENANT_ID) throw new Error('Must set env variable $REACT_APP_MSAL_TENANT_ID');
            return 'https://login.microsoftonline.com/' + process.env.REACT_APP_MSAL_TENANT_ID;
        }
        
        return 'https://login.microsoftonline.com/__REACT_APP_MSAL_TENANT_ID__';
    };

    _getScope = () => {
        if (process.env.NODE_ENV !== 'production') {
            if (!process.env.REACT_APP_MSAL_SCOPE) throw new Error('Must set env variable REACT_APP_MSAL_SCOPE');
            return process.env.REACT_APP_MSAL_SCOPE;
        }
        
        return '__REACT_APP_MSAL_SCOPE__';
    };

    configuration: Configuration = {
        auth: {
            clientId: this._getClientId(),
            authority: this._getAuthority(),
            redirectUri: window.location.origin,
            postLogoutRedirectUri: window.location.origin,
            navigateToLoginRequestUrl: true,
        },
        cache: {
            cacheLocation: 'sessionStorage',
            storeAuthStateInCookie: false
        }
    };

    clientApplication = new PublicClientApplication(this.configuration);

    getScopes = (scopes: string | string[] = 'openid', parseScopes: boolean = true): string[] => {

        const oidScope = 'openid'
        const hostScope = '{$host}/.default';
        const tcwebScope = this._getScope();// 'http://TeamCloud.Web/user_impersonation';

        if (!Array.isArray(scopes))
            scopes = [scopes];

        if (parseScopes) {

            const hostIndex = scopes.indexOf(hostScope);

            if (hostIndex >= -1)
                scopes.splice(hostIndex, 1)

            if (!scopes.includes(oidScope))
                scopes.push(oidScope);

            if (!scopes.includes(tcwebScope))
                scopes.push(tcwebScope);
        }

        return scopes;
    }

    getManagementToken = async (): Promise<AccessToken | null> => {

        const scopes = ['https://management.azure.com/.default'];

        const accounts = this.clientApplication.getAllAccounts();

        if (accounts.length <= 0) {
            console.error('nope')
            return null;
        }

        const account = accounts[0];

        var authResult = await this.clientApplication.acquireTokenSilent({ account: account, scopes: scopes as string[] });

        return { token: authResult.accessToken, expiresOnTimestamp: authResult.expiresOn!.getTime() };
    }

    getToken = async (scopes: string | string[] = 'openid'): Promise<AccessToken | null> => {

        scopes = this.getScopes(scopes);

        // console.log(`getToken (${scopes.includes('User.Read') ? 'graph' : 'api'})`);
        const accounts = this.clientApplication.getAllAccounts();

        if (accounts.length <= 0) {
            console.error('nope')
            return null;
        }
        const account = accounts[0];

        try {

            var authResult = await this.clientApplication.acquireTokenSilent({ account: account, scopes: scopes as string[] });

            return { token: authResult.accessToken, expiresOnTimestamp: authResult.expiresOn!.getTime() };

        } catch (error) {

            if (error instanceof InteractionRequiredAuthError) {
                // console.error(error);
                console.error(`errorCode : ${error.errorCode}`);
                console.error(`errorMessage : ${error.errorMessage}`);
                console.error(`message : ${error.message}`);
                console.error(`name : ${error.name}`);
                console.error(`subError : ${error.subError}`);

                try {

                    await this.clientApplication.acquireTokenRedirect({ account: account, scopes: scopes as string[] })

                } catch (err) {

                    if (err instanceof InteractionRequiredAuthError) {
                        console.error(`err.errorCode : ${err.errorCode}`);
                        console.error(`err.errorMessage : ${err.errorMessage}`);
                        console.error(`err.message : ${err.message}`);
                        console.error(`err.name : ${err.name}`);
                        console.error(`err.subError : ${err.subError}`);
                    }
                }
            }

            return null;
        }

        // console.log('TOKEN (' + (authParameters.scopes || []).join(' | ') + ' | ' + authResponse.expiresOn + ') ' + authResponse.accessToken);
    }

    getAccessToken = async (authenticationProviderOptions?: AuthenticationProviderOptions): Promise<string> => {
        const graphScopes = ['User.Read', 'User.ReadBasic.All', 'Directory.Read.All', 'People.Read']; // An array of graph scopes

        if (authenticationProviderOptions?.scopes)
            graphScopes.concat(authenticationProviderOptions.scopes)

        const token = await this.getToken(graphScopes);

        return token?.token ?? Promise.reject('Unable to get token');
    }

    logout = async (): Promise<void> => this.clientApplication.logout();
}

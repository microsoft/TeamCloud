// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Configuration, AuthenticationParameters } from 'msal';
import { MsalAuthProvider, LoginType, IMsalAuthProviderConfig } from 'react-aad-msal';
import { AccessToken, TokenCredential } from '@azure/core-auth'

export class Auth implements TokenCredential {



    constructor() {

    }

    _getClientId = () => {
        if (!process.env.REACT_APP_MSAL_CLIENT_ID) throw new Error('Must set env variable $REACT_APP_MSAL_CLIENT_ID');
        return process.env.REACT_APP_MSAL_CLIENT_ID;
    };

    _getAuthority = () => {
        if (!process.env.REACT_APP_MSAL_TENANT_ID) throw new Error('Must set env variable $REACT_APP_MSAL_TENANT_ID');
        return 'https://login.microsoftonline.com/' + process.env.REACT_APP_MSAL_TENANT_ID;
    };

    configuration: Configuration = {
        auth: {
            clientId: this._getClientId(),
            authority: this._getAuthority(),
            redirectUri: window.location.origin,
            postLogoutRedirectUri: window.location.origin,
            navigateToLoginRequestUrl: true,
            validateAuthority: false,
        },
        cache: {
            cacheLocation: 'sessionStorage',
            storeAuthStateInCookie: false
        }
    };

    parameters: AuthenticationParameters = { scopes: ['openid'] }

    options: IMsalAuthProviderConfig = { loginType: LoginType.Redirect }

    authProvider = new MsalAuthProvider(this.configuration, this.parameters, this.options)

    getToken = async (scopes: string | string[] = 'openid'): Promise<AccessToken | null> => {

        const oidScope = 'openid'
        const hostScope = '{$host}/.default';
        const tcwebScope = 'http://TeamCloud.Web/user_impersonation';
        // console.log(authProvider.UserAgentApplication.getAccount());

        // if (reAuth) { await authProvider.getAccessToken(parameters); }

        if (!Array.isArray(scopes))
            scopes = [scopes];

        const hostIndex = scopes.indexOf(hostScope);
        if (hostIndex >= -1)
            scopes.splice(hostIndex, 1)

        if (!scopes.includes(oidScope))
            scopes.push(oidScope);

        if (!scopes.includes(tcwebScope))
            scopes.push(tcwebScope);

        scopes.forEach(scope => console.warn(`scope: ${scope}`));

        var authParameters: AuthenticationParameters = { scopes: scopes };
        var authResponse = await this.authProvider.getAccessToken(authParameters);

        // console.log('TOKEN (' + (authParameters.scopes || []).join(' | ') + ' | ' + authResponse.expiresOn + ') ' + authResponse.accessToken);

        return { token: authResponse.accessToken, expiresOnTimestamp: authResponse.expiresOn.getTime() };
    }
}


// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Configuration, AuthenticationParameters } from 'msal';
import { MsalAuthProvider, LoginType, IMsalAuthProviderConfig } from 'react-aad-msal';

const _getClientId = () => {
    if (!process.env.REACT_APP_MSAL_CLIENT_ID) throw new Error('Must set env variable $REACT_APP_MSAL_CLIENT_ID');
    return process.env.REACT_APP_MSAL_CLIENT_ID;
};

const _getAuthority = () => {
    if (!process.env.REACT_APP_MSAL_TENANT_ID) throw new Error('Must set env variable $REACT_APP_MSAL_TENANT_ID');
    return 'https://login.microsoftonline.com/' + process.env.REACT_APP_MSAL_TENANT_ID;
};

const configuration: Configuration = {
    auth: {
        clientId: _getClientId(),
        authority: _getAuthority(),
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

const parameters: AuthenticationParameters = { scopes: ['openid'] }

const options: IMsalAuthProviderConfig = { loginType: LoginType.Redirect }

export const authProvider = new MsalAuthProvider(configuration, parameters, options)

export const getToken = async (scope: string = 'openid', reAuth?: boolean) => {

    // console.log(authProvider.UserAgentApplication.getAccount());

    if (reAuth) { await authProvider.getAccessToken(parameters); }

    var authParameters: AuthenticationParameters = { scopes: [scope] };
    var authResponse = await authProvider.getAccessToken(authParameters);

    // console.log('TOKEN (' + (authParameters.scopes || []).join(' | ') + ' | ' + authResponse.expiresOn + ') ' + authResponse.accessToken);

    return authResponse.accessToken;
}

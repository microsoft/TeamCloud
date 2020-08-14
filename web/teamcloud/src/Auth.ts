import { Configuration, AuthenticationParameters } from 'msal';
import { MsalAuthProvider, LoginType, IMsalAuthProviderConfig } from 'react-aad-msal';

export const authScopeDefault: string = 'https://management.core.windows.net//user_impersonation';
export const authScopeAzure: string = 'https://management.core.windows.net//user_impersonation';
export const authScopeUser: string = 'https://graph.microsoft.com/User.Read';
export const authScopeProfile: string = 'https://graph.microsoft.com/User.Read';

const authenticationConfiguration: Configuration = {
    auth: {
        authority: 'https://login.microsoftonline.com/common',
        clientId: '[your client id goes here]',
        redirectUri: window.location.origin,
        postLogoutRedirectUri: window.location.origin,
        validateAuthority: false,
        navigateToLoginRequestUrl: true,
    },
    cache: {
        cacheLocation: "sessionStorage",
        storeAuthStateInCookie: false
    }
};

const authenticationParameters: AuthenticationParameters = {
    scopes: [
        'openid'
    ]
}

const options: IMsalAuthProviderConfig = {
    loginType: LoginType.Redirect
}

export const authProvider = new MsalAuthProvider(authenticationConfiguration, authenticationParameters, options)

export const getToken = async (scope: string = 'openid', reAuth?: boolean) => {

    // console.log(authProvider.UserAgentApplication.getAccount());

    if (reAuth) { await authProvider.getAccessToken(authenticationParameters); }

    var authParameters: AuthenticationParameters = { scopes: [scope] };
    var authResponse = await authProvider.getAccessToken(authParameters);

    // console.log('TOKEN (' + (authParameters.scopes || []).join(' | ') + ' | ' + authResponse.expiresOn + ') ' + authResponse.accessToken);

    return authResponse.accessToken;
}

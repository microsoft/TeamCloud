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
        postLogoutRedirectUri: window.location.origin,
        // redirectUri: window.location.origin,
        redirectUri: 'http://localhost:3000',
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
        // 'user_impersonation',
        'openid',
        // authScopeAzure,
        // authScopeAzure,
        // authScopeUser,
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

export const isMSA = () => {

    const msaIssuerId: string = "9188040d-6c67-4c5b-b112-36a304b66dad";
    // return authProvider.getAccount().idToken.iss.indexOf(msaIssuerId) > -1;
    const iss = authProvider.getAccountInfo()?.account.idToken.iss;
    return iss !== undefined && iss!.indexOf(msaIssuerId) > -1;
}

export interface GraphUser {
    id: string;
    givenName: string;
    surname: string;
    displayName: string;
    mail: string;
    jobTitle: string;
    userPrincipalName: string;
}

export const getGraphUser = async (): Promise<GraphUser> => {

    if (isMSA()) {
        var account = authProvider.getAccount();
        return {
            id: account.accountIdentifier,
            displayName: account.name,
            userPrincipalName: account.userName
        } as GraphUser;
    }

    let response: Response = await fetch('https://graph.microsoft.com/v1.0/me', {
        method: 'GET',
        mode: 'cors',
        headers: {
            'Authorization': 'Bearer ' + await getToken(authScopeUser)
        }
    });

    // let json = await response.json();
    // console.log("=== JSON (" + 'https://graph.microsoft.com/v1.0/me' + ") " + JSON.stringify(json));

    return await response.json() as GraphUser;
}

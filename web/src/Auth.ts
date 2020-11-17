// import { MsalProvider } from '@azure/msal-react';
import { Configuration, PublicClientApplication } from '@azure/msal-browser';
import { AccessToken, TokenCredential } from '@azure/core-auth'
import { AuthenticationProvider, AuthenticationProviderOptions } from "@microsoft/microsoft-graph-client";

export class Auth implements TokenCredential, AuthenticationProvider {

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
        },
        cache: {
            cacheLocation: 'sessionStorage',
            storeAuthStateInCookie: false
        }
    };

    clientApplication = new PublicClientApplication(this.configuration);

    getScopes = (scopes: string | string[] = 'openid'): string[] => {

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

        return scopes;
    }

    getToken = async (scopes: string | string[] = 'openid'): Promise<AccessToken | null> => {

        scopes = this.getScopes(scopes);

        console.warn(`getToken (${scopes.includes('User.Read') ? 'graph' : 'api'})`);
        // scopes.forEach(scope => console.warn(`scope: ${scope}`));

        // var authParameters: AuthenticationParameters = { scopes: scopes };
        // var authResponse = await this.authProvider.getAccessToken(authParameters);

        if (this.clientApplication.getAllAccounts().length <= 0) {
            console.error('nope')
            return null;
        }

        const account = this.clientApplication.getAllAccounts()[0];

        var authResult = await this.clientApplication.acquireTokenSilent({ account: account, scopes: scopes as string[] });
        // const tokenResponse = await this.clientApplication.acquireTokenSilent({ scopes: scopes }).catch(error => {
        //     if (error instanceof InteractionRequiredAuthError) {
        //         // fallback to interaction when silent call fails
        //         return myMSALObj.acquireTokenRedirect(request)
        //     }
        // });

        // console.log('TOKEN (' + (authParameters.scopes || []).join(' | ') + ' | ' + authResponse.expiresOn + ') ' + authResponse.accessToken);

        return { token: authResult.accessToken, expiresOnTimestamp: authResult.expiresOn.getTime() };
    }

    getAccessToken = async (authenticationProviderOptions?: AuthenticationProviderOptions): Promise<string> => {
        const graphScopes = ['User.Read', 'User.ReadBasic.All', 'Directory.Read.All', 'People.Read']; // An array of graph scopes

        if (authenticationProviderOptions?.scopes)
            graphScopes.concat(authenticationProviderOptions.scopes)

        const token = await this.getToken(graphScopes);

        return token?.token ?? Promise.reject('Unable to get token');
    }
}

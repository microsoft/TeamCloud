import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import * as serviceWorker from './serviceWorker';
import { AzureAD, AuthenticationState, IAzureADFunctionProps } from 'react-aad-msal';
import { authProvider } from './Auth';
// import { Customizer } from '@fluentui/react';
import { Error403 } from './view';


// import FluentCustomizations from '@'

ReactDOM.render(
  <React.StrictMode>
    <AzureAD provider={authProvider} forceLogin={true}>
      {({ login, logout, authenticationState, error, accountInfo }: IAzureADFunctionProps) => {
        if (authenticationState === AuthenticationState.Authenticated) {
          // var tenantIdRegex = /(\{){0,1}[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}(\}){0,1}/gi;
          // var tenantIdMatch = tenantIdRegex.exec(accountInfo?.account.environment ?? '');
          // var tenantId = tenantIdMatch ? tenantIdMatch[0] : '00000000-0000-0000-0000-000000000000';
          // console.log(accountInfo);
          // console.log(authProvider);
          return <App onSignOut={logout} />
        } else if (authenticationState === AuthenticationState.Unauthenticated) {
          return <Error403 error={error} />
        }
        return null;
      }}
    </AzureAD>
  </React.StrictMode>,
  document.getElementById('root')
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();

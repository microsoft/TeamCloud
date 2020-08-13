import React from 'react';
import ReactDOM from 'react-dom';
import { AzureAD, AuthenticationState, IAzureADFunctionProps } from 'react-aad-msal';
import * as serviceWorker from './serviceWorker';
import { authProvider } from './Auth';
import { Error403 } from './view';
import App from './App';
import './index.css'

ReactDOM.render(
    <React.StrictMode>
        <AzureAD provider={authProvider} forceLogin={true}>
            {({ login, logout, authenticationState, error, accountInfo }: IAzureADFunctionProps) => {
                if (authenticationState === AuthenticationState.Authenticated)
                    return <App onSignOut={logout} />
                else if (authenticationState === AuthenticationState.Unauthenticated)
                    return <Error403 error={error} />
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

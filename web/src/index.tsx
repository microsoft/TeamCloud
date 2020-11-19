// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import ReactDOM from 'react-dom';
import { MsalProvider } from '@azure/msal-react';
import * as serviceWorker from './serviceWorker';
import { auth } from './API';
import App from './App';

import './index.css'

ReactDOM.render(
    <MsalProvider instance={auth.clientApplication}>
        <App />
    </MsalProvider>
    , document.getElementById('root')
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();

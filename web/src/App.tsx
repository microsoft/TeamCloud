// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { RootView } from './view';

export const App: React.FC = () => (
    <BrowserRouter>
        <RootView />
    </BrowserRouter>
);

export default App;

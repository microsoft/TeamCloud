// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { QueryClient, QueryClientProvider } from 'react-query';
import { ReactQueryDevtools } from 'react-query/devtools'
import { BrowserRouter } from 'react-router-dom';
import { RootView } from './view';

const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            refetchOnMount: false,
            refetchOnWindowFocus: false,
            // staleTime: 100 * 50 * 1
        }
    }
})

export const App: React.FC = () => (
    <QueryClientProvider client={queryClient}>
        <BrowserRouter>
            <RootView />
        </BrowserRouter>
        <ReactQueryDevtools initialIsOpen={false} position='bottom-right' />
    </QueryClientProvider>
);

export default App;

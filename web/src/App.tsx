// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { QueryClient, QueryClientProvider } from 'react-query';
import { ReactQueryDevtools } from 'react-query/devtools'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { RootView } from './view';
import './App.css'

const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            refetchOnMount: false,
            refetchOnWindowFocus: false,
            staleTime: 1000 * 60 * 1
        }
    }
})

export const App: React.FC = () => (
    <QueryClientProvider client={queryClient}>
        <BrowserRouter>
            <Routes>
                <Route path='/orgs' element={<Navigate to='/' replace />} />
                <Route path='/orgs/:orgId/projects' element={<Navigate replace to='/orgs/:orgId' />} />
                <Route path='/orgs/:orgId/settings/overview' element={<Navigate replace to='/orgs/:orgId/settings' />} />
                <Route path='/orgs/:orgId/projects/:projectId/overview' element={<Navigate replace to='/orgs/:orgId/projects/:projectId' />} />
                <Route path='/orgs/:orgId/projects/:projectId/settings/overview' element={<Navigate replace to='/orgs/:orgId/projects/:projectId/settings' />} />
                <Route path='/orgs/:orgId/projects/:projectId/components/:itemId/tasks' element={<Navigate replace to='/orgs/:orgId/projects/:projectId/components/:itemId' />} />
                <Route path='/*' element={<RootView />} />
            </Routes>
        </BrowserRouter>
        <ReactQueryDevtools initialIsOpen={false} position='bottom-right' />
    </QueryClientProvider>
);

export default App;

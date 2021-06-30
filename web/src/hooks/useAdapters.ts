// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';

export const useAdapters = () => {

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['adapters'], async () => {
        const { data } = await api.getAdapters();
        return data;
    }, {
        enabled: isAuthenticated
    });
}

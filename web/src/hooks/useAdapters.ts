// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api, onResponse } from '../API';

export const useAdapters = () => {

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['adapters'], async () => {
        const { data } = await api.getAdapters({
            onResponse: onResponse
        });
        return data;
    }, {
        enabled: isAuthenticated
    });
}

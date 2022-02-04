// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api, onResponse } from '../API';

export const useOrgs = () => {

    const isAuthenticated = useIsAuthenticated();

    return useQuery('orgs', async () => {

        const { data } = await api.getOrganizations({
            onResponse: onResponse
        });

        return data
    }, {
        enabled: isAuthenticated
    });
}

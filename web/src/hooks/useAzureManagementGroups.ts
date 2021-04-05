// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { getManagementGroups } from '../Azure';

export const useAzureManagementGroups = () => {

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['azure', 'managementGroups'], async () => {
        try {
            const groups = await getManagementGroups();
            return groups ?? [];
        } catch (error) {
            console.error(error);
            return [];
        }
    }, {
        enabled: isAuthenticated
    });
}

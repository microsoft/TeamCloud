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
            return (groups ?? []).sort((mg1, mg2) =>{
                if ((mg1.displayName ?? mg1.name) < (mg2.displayName ?? mg2.name)) {
                    return -1;
                } else if ((mg1.displayName ?? mg1.name) > (mg2.displayName ?? mg2.name)) {
                    return 1;
                } else {
                    return 0;
                }
            });
        } catch (error) {
            console.error(error);
            return [];
        }
    }, {
        enabled: isAuthenticated
    });
}

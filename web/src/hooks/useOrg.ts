// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api, onResponse } from '../API';
import { useUrl } from '.';

export const useOrg = () => {

    const { orgId } = useUrl() as { orgId: string };

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['org', orgId], async () => {

        const { data } = await api.getOrganization(orgId, {
            onResponse: onResponse
        });

        return data;
    }, {
        enabled: isAuthenticated && !!orgId
    });
}

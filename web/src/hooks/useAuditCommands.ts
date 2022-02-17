// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api, onResponse } from '../API';
import { useOrg } from '.';

export const useAuditCommands = () => {

    const { data: org } = useOrg();

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['org', org?.id, 'audit', 'commands'], async () => {

        const { data } = await api.getAuditCommands(org!.id, {
            onResponse: onResponse
        });
        return data;
    }, {
        enabled: isAuthenticated && !!org?.id
    });
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useOrg } from '.';
import { ErrorResult } from 'teamcloud';

export const useAuditEntries = (timeRange?: string, commands?: string[]) => {

    const { data: org } = useOrg();

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['org', org?.id, 'audit'], async () => {

        const { data, code, _response } = await api.getAuditEntries(org!.id, {
            timeRange: timeRange,
            commands: commands
        });

        if (code && code >= 400) {
            const error = JSON.parse(_response.bodyAsText) as ErrorResult;
            throw error;
        }

        return data;
    }, {
        enabled: isAuthenticated && !!org?.id
    });
}

export const useAuditCommands = () => {

    const { data: org } = useOrg();

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['org', org?.id, 'audit', 'commands'], async () => {

        const { data, code, _response } = await api.getAuditCommands(org!.id);

        if (code && code >= 400) {
            const error = JSON.parse(_response.bodyAsText) as ErrorResult;
            throw error;
        }

        return data;
    }, {
        enabled: isAuthenticated && !!org?.id
    });
}
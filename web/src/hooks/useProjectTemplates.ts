// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useOrg } from '.';

export const useProjectTemplates = () => {

    const { data: org } = useOrg();

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['org', org?.id, 'templates'], async () => {

        const { data } = await api.getProjectTemplates(org!.id, {
            onResponse: (raw, flat) => {
                if (raw.status >= 400)
                    throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
            }
        });

        return data;
    }, {
        enabled: isAuthenticated && !!org?.id
    });
}

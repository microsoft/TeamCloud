// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useParams } from 'react-router-dom';
import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useOrg } from '.';

export const useProject = () => {

    const { projectId } = useParams() as { projectId: string };

    const isAuthenticated = useIsAuthenticated();

    const { data: org } = useOrg();

    return useQuery(['org', org?.id, 'project', projectId], async () => {

        const { data } = await api.getProject(projectId, org!.id, {
            onResponse: (raw, flat) => {
                if (raw.status >= 400)
                    throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
            }
        });

        return data;
    }, {
        enabled: isAuthenticated && !!org?.id && !!projectId
    });
}

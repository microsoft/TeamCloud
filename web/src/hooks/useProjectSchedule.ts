// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useProject } from '.';

export const useProjectSchedule = () => {

    const { settingId, itemId } = useParams() as { settingId: string, itemId: string };

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();

    return useQuery(['org', project?.organization, 'project', project?.id, 'schedule', itemId], async () => {

        const { data } = await api.getSchedule(itemId, project!.organization, project!.id, {
            onResponse: (raw, flat) => {
                if (raw.status >= 400)
                    throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
            }
        });

        return data;
    }, {
        enabled: isAuthenticated && !!project?.id && !!settingId && settingId.toLowerCase() === 'schedules' && !!itemId
    });
}

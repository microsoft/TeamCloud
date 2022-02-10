// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api, onResponse } from '../API';
import { useProject, useUrl } from '.';

export const useProjectSchedule = () => {

    const { settingId, itemId } = useUrl() as { settingId: string, itemId: string };

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();

    return useQuery(['org', project?.organization, 'project', project?.id, 'schedule', itemId], async () => {

        const { data } = await api.getSchedule(itemId, project!.organization, project!.id, {
            onResponse: onResponse
        });

        return data;
    }, {
        enabled: isAuthenticated && !!project?.id && !!settingId && settingId.toLowerCase() === 'schedules' && !!itemId
    });
}

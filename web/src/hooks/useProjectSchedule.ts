// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useProject } from '.';
import { ErrorResult } from 'teamcloud';

export const useProjectSchedule = () => {

    const { settingId, itemId } = useParams() as { settingId: string, itemId: string };

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();

    return useQuery(['org', project?.organization, 'project', project?.id, 'schedule', itemId], async () => {

        const { data, code, _response } = await api.getSchedule(itemId, project!.organization, project!.id);

        if (code && code >= 400) {
            const error = JSON.parse(_response.bodyAsText) as ErrorResult;
            throw error;
        }

        return data;
    }, {
        enabled: isAuthenticated && !!project?.id && !!settingId && settingId.toLowerCase() === 'schedules' && !!itemId
    });
}

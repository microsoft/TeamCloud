// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useProject } from '.';

export const useProjectSchedules = () => {

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();

    return useQuery(['org', project?.organization, 'project', project?.id, 'schedule'], async () => {
        const { data } = await api.getSchedules(project!.organization, project!.id);
        return data;
    }, {
        enabled: isAuthenticated && !!project?.id
    });
}

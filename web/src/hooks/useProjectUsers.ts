// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api, onResponse } from '../API';
import { useProject } from '.';

export const useProjectUsers = () => {

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();

    return useQuery(['org', project?.organization, 'project', project?.id, 'user'], async () => {

        const { data } = await api.getProjectUsers(project!.organization, project!.id, {
            onResponse: onResponse
        });

        return data;
    }, {
        enabled: isAuthenticated && !!project?.id
    });
}

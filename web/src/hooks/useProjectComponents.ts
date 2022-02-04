// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api, onResponse } from '../API';
import { useProject } from '.';

export const useProjectComponents = () => {

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();

    return useQuery(['org', project?.organization, 'project', project?.id, 'component'], async () => {

        const { data } = await api.getComponents(project!.organization, project!.id, {
            onResponse: onResponse
        });

        return data;
    }, {
        enabled: isAuthenticated && !!project?.id
    });
}

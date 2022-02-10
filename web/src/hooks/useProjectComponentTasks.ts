// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { matchesRouteParam } from '../Utils';
import { api, onResponse } from '../API';
import { useProject, useProjectComponent, useUrl } from '.';

export const useProjectComponentTasks = () => {

    const { itemId } = useUrl() as { itemId: string };

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();
    const { data: component } = useProjectComponent();

    return useQuery(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask'], async () => {

        const { data } = await api.getComponentTasks(project!.organization, project!.id, component!.id, {
            onResponse: onResponse
        });

        return data;
    }, {
        enabled: isAuthenticated && !!project?.id && !!component?.id && !!itemId && matchesRouteParam(component, itemId)
    });
}

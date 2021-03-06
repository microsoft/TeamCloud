// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { matchesRouteParam } from '../Utils';
import { api } from '../API';
import { useProject, useProjectComponent } from '.';

export const useProjectComponentTask = () => {

    const { itemId, subitemId } = useParams() as { itemId: string, subitemId: string };

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();
    const { data: component } = useProjectComponent();

    return useQuery(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask', subitemId], async () => {

        const { data } = await api.getComponentTask(subitemId, project!.organization, project!.id, component!.id, {
            onResponse: (raw, flat) => {
                if (raw.status >= 400)
                    throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
            }
        });

        return data;
    }, {
        enabled: isAuthenticated && !!project?.id && !!component?.id && !!itemId && matchesRouteParam(component, itemId) && !!subitemId,
    });
}

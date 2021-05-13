// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { matchesRouteParam } from '../Utils';
import { api } from '../API';
import { useProject, useProjectComponent } from '.';
import { ErrorResult } from 'teamcloud';

export const useProjectComponentTask = () => {

    const { itemId, subitemId } = useParams() as { itemId: string, subitemId: string };

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();
    const { data: component } = useProjectComponent();

    return useQuery(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask', subitemId], async () => {

        const { data, code, _response } = await api.getComponentTask(subitemId, project!.organization, project!.id, component!.id);

        if (code && code >= 400) {
            const error = JSON.parse(_response.bodyAsText) as ErrorResult;
            throw error;
        }

        return data;
    }, {
        enabled: isAuthenticated && !!project?.id && !!component?.id && !!itemId && matchesRouteParam(component, itemId) && !!subitemId,
    });
}

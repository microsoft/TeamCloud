// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { matchesLowerCase, matchesRouteParam } from '../Utils';
import { api } from '../API';
import { useProject, useProjectComponent } from '.';

export const useProjectComponentTask = () => {

    const { navId, itemId, subitemId } = useParams() as { navId: string, itemId: string, subitemId: string };

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();
    const { data: component } = useProjectComponent();

    return useQuery(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask', subitemId], async () => {
        const { data } = await api.getComponentTask(subitemId, project!.organization, project!.id, component!.id);
        return data;;
    }, {
        enabled: isAuthenticated && !!project?.id && !!navId && matchesLowerCase(navId, 'components') && !!component?.id && !!itemId && matchesRouteParam(component, itemId) && !!subitemId
    });
}

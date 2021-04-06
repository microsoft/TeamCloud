// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { matchesRouteParam } from '../Utils';
import { api } from '../API';
import { useProject, useProjectComponent } from '.';
// import { ComponentTask } from 'teamcloud';

export const useProjectComponentTask = () => {

    const { itemId, subitemId } = useParams() as { itemId: string, subitemId: string };

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();
    const { data: component } = useProjectComponent();

    // const queryClient = useQueryClient();

    return useQuery(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask', subitemId], async () => {
        const { data } = await api.getComponentTask(subitemId, project!.organization, project!.id, component!.id);
        return data;;
    }, {
        enabled: isAuthenticated && !!project?.id && !!component?.id && !!itemId && matchesRouteParam(component, itemId) && !!subitemId,
        // onSuccess: data => {
        //     if (data) {
        //         const componentTasks = queryClient.getQueryData<ComponentTask[]>(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask']);
        //         if (componentTasks) {
        //             const index = componentTasks.findIndex(t => t.id === data.id);
        //             const newComponentTasks = [...componentTasks];
        //             if (index >= 0) {
        //                 newComponentTasks[index] = data;
        //             } else {
        //                 newComponentTasks.push(data);
        //             }
        //             queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask'], newComponentTasks);
        //         }
        //     }
        // }
    });
}

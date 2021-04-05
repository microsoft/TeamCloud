// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery, useQueryClient } from 'react-query'
import { useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { matchesRouteParam } from '../Utils';
import { api } from '../API';
import { useProject, useProjectComponent, useProjectComponentTask, useProjectComponentTasks } from '.';

export const useProjectComponentTaskPoll = () => {

    const { itemId, subitemId } = useParams() as { itemId: string, subitemId: string };

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();
    const { data: component } = useProjectComponent();
    const { data: componentTask } = useProjectComponentTask();
    const { data: componentTasks } = useProjectComponentTasks();

    const queryClient = useQueryClient();


    return useQuery(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask', subitemId, 'poll'], async () => {
        const { data } = await api.getComponentTask(subitemId, project!.organization, project!.id, component!.id);
        return data;;
    }, {
        refetchInterval: componentTask?.resourceState !== undefined && (componentTask.resourceState.toLowerCase() === 'succeeded' || componentTask.resourceState.toLowerCase() === 'failed') ? false : 2000,
        enabled: isAuthenticated && !!project?.id && !!component?.id && !!itemId && matchesRouteParam(component, itemId) && !!subitemId && componentTask && !componentTask.exitCode,
        onSuccess: data => {
            if (data) {
                queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask', subitemId], data);
                if (componentTasks) {
                    const index = componentTasks.findIndex(t => t.id === data.id);
                    if (index >= 0) {
                        const newComponentTasks = [...componentTasks];
                        newComponentTasks[index] = data;
                        queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask'], newComponentTasks);
                    }
                }
            }
        }
    });
}

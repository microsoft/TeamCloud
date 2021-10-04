// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { useHistory, useParams } from 'react-router-dom';
import { ComponentTaskDefinition } from 'teamcloud';
import { api } from '../API';
import { useOrg, useProject, useProjectComponent, useProjectComponentTasks } from '.';

export const useCreateProjectComponentTask = () => {

    const history = useHistory();

    const { orgId, projectId, itemId } = useParams() as { orgId: string, projectId: string, itemId: string };

    const { data: org } = useOrg();
    const { data: project } = useProject();
    const { data: component } = useProjectComponent();
    const { data: componentTasks } = useProjectComponentTasks();

    const queryClient = useQueryClient();

    return useMutation(async (componentTaskDef: ComponentTaskDefinition) => {
        if (!project) throw Error('No project')
        if (!component) throw Error('No component')

        const { data } = await api.createComponentTask(project.organization, project.id, component.id, {
            body: componentTaskDef,
            onResponse: (raw, flat) => {
                if (raw.status >= 400)
                    throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
            }
        });

        return data;
    }, {
        onSuccess: data => {
            if (data) {
                queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask', data.id], data)
                queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask'], componentTasks ? [...componentTasks, data] : [data])

                history.push(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/components/${component?.slug ?? itemId}/tasks/${data.id}`);
            }
        }
    }).mutateAsync
}


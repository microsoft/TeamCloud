// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { Component } from 'teamcloud';
import { api, onResponse } from '../API';
import { useProject } from '.';

export const useDeleteProjectComponent = () => {

    const { data: project } = useProject();

    const queryClient = useQueryClient();

    return useMutation(async (component: Component) => {
        if (!project) throw Error('No project')

        const result = await api.deleteComponent(component.id, project.organization, project.id, {
            onResponse: onResponse
        });
        return result
    }, {
        onSuccess: (data, component) => {
            queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'component', component.slug], undefined)
            queryClient.invalidateQueries(['org', project?.organization, 'project', project?.id, 'component'])

            // navigate(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/components/${component?.slug}`);
        }
    }).mutateAsync
}

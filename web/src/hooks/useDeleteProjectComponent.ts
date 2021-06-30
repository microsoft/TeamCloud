// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { Component, ErrorResult } from 'teamcloud';
import { api } from '../API';
import { useProject } from '.';

export const useDeleteProjectComponent = () => {

    const { data: project } = useProject();

    const queryClient = useQueryClient();

    return useMutation(async (component: Component) => {
        if (!project) throw Error('No project')

        const result = await api.deleteComponent(component.id, project.organization, project.id);
        if (result.code !== 202 && (result as ErrorResult).errors) {
            console.log(result as ErrorResult);
        }
        return undefined;
    }, {
        onSuccess: (data, component) => {
            queryClient.invalidateQueries(['org', project?.organization, 'project', project?.id, 'component'])
            queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'component', component.slug], data)

            // history.push(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/components/${component?.slug}`);
        }
    }).mutateAsync
}

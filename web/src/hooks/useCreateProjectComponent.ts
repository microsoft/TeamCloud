// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { useHistory, useParams } from 'react-router-dom';
import { ComponentDefinition, ErrorResult } from 'teamcloud';
import { api } from '../API';
import { useOrg, useProject, useProjectComponents } from '.';

export const useCreateProjectComponent = () => {

    const history = useHistory();

    const { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const { data: org } = useOrg();
    const { data: project } = useProject();
    const { data: components } = useProjectComponents();

    const queryClient = useQueryClient();

    return useMutation(async (componentDef: ComponentDefinition) => {
        if (!project) throw Error('No project')

        const { data, code, _response } = await api.createComponent(project.organization, project.id, { body: componentDef });

        if (code && code >= 400) {
            const error = JSON.parse(_response.bodyAsText) as ErrorResult;
            throw error;
        }

        return data;
    }, {
        onSuccess: data => {
            if (data) {
                queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'component', data.slug], data)
                queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'component'], components ? [...components, data] : [data])

                history.push(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/components/${data?.slug}`);
            }
        }
    }).mutateAsync
}

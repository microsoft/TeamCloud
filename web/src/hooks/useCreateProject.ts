// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { useHistory } from 'react-router-dom';
import { ProjectDefinition } from 'teamcloud';
import { api } from '../API';
import { useOrg, useProjects } from '.';

export const useCreateProject = () => {

    const history = useHistory();

    const { data: org } = useOrg();
    const { data: projects } = useProjects();

    const queryClient = useQueryClient();

    return useMutation(async (projectDef: ProjectDefinition) => {
        if (!org) throw Error('No Org');

        const { data } = await api.createProject(org.id, { body: projectDef });
        return data;
    }, {
        onSuccess: data => {
            if (data && org) {
                queryClient.setQueryData(['org', org.id, 'project', data.slug], data);
                queryClient.setQueryData(['org', org.id, 'projects'], projects ? [...projects, data] : [data]);

                history.push(`/orgs/${org.slug}/projects/${data.slug}`);
            }
        }
    }).mutateAsync
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { useNavigate } from 'react-router-dom';
import { ProjectDefinition } from 'teamcloud';
import { api, onResponse } from '../API';
import { useOrg, useProjects } from '.';

export const useCreateProject = () => {

    const navigate = useNavigate();

    const { data: org } = useOrg();
    const { data: projects } = useProjects();

    const queryClient = useQueryClient();

    return useMutation(async (projectDef: ProjectDefinition) => {
        if (!org) throw Error('No Org');

        const { data } = await api.createProject(org.id, {
            body: projectDef,
            onResponse: onResponse
        });

        return data;
    }, {
        onSuccess: data => {
            if (data && org) {
                queryClient.setQueryData(['org', org.id, 'project', data.slug], data);
                queryClient.setQueryData(['org', org.id, 'projects'], projects ? [...projects, data] : [data]);

                navigate(`/orgs/${org.slug}/projects/${data.slug}`);
            }
        },
    }).mutateAsync
}

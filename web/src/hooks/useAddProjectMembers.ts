// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { UserDefinition } from 'teamcloud';
import { api } from '../API';
import { getGraphUser } from '../MSGraph';
import { useProject, useProjectMembers } from '.';

export const useAddProjectMembers = () => {

    const { data: project } = useProject();
    const { data: members } = useProjectMembers();

    const queryClient = useQueryClient();

    return useMutation(async (users: UserDefinition[]) => {
        if (!project) throw Error('No project')

        const responses = await Promise
            .all(users.map(async d => await api.createProjectUser(project.organization, project.id, { body: d })));

        responses.forEach(r => {
            if (!r.data)
                console.error(r);
        });

        const newMembers = await Promise.all(responses
            .filter(r => r.data)
            .map(r => r.data!)
            .map(async u => ({
                user: u,
                graphUser: await getGraphUser(u.id),
                projectMembership: u.projectMemberships!.find(m => m.projectId === project.id)!
            })));

        return newMembers;
    }, {
        onSuccess: data => {
            queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'user'], members ? [...members, ...data] : data)
        }
    }).mutateAsync
}

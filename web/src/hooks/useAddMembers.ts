// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { UserDefinition } from 'teamcloud';
import { api } from '../API';
import { getGraphUser } from '../MSGraph';
import { useOrg, useMembers } from '.';

export const useAddMembers = () => {

    const { data: org } = useOrg();
    const { data: members } = useMembers();

    const queryClient = useQueryClient();

    return useMutation(async (users: UserDefinition[]) => {
        if (!org) throw Error('No Org');

        const responses = await Promise
            .all(users.map(async d => await api.createOrganizationUser(org.id, { body: d })));

        responses.forEach(r => {
            if (!r.data)
                console.error(r);
        });

        const newMembers = await Promise.all(responses
            .filter(r => r.data)
            .map(r => r.data!)
            .map(async u => ({
                user: u,
                graphUser: await getGraphUser(u.id)
            })));

        return newMembers;
    }, {
        onSuccess: data => {
            // queryClient.getQueryData(['org', org!.id, 'members'])
            queryClient.setQueryData(['org', org!.id, 'members'], members ? [...members, ...data] : data);
        }
    }).mutateAsync
}

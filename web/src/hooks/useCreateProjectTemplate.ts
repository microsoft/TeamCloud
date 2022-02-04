// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { ProjectTemplateDefinition } from 'teamcloud';
import { api, onResponse } from '../API';
import { useOrg } from '.';

export const useCreateProjectTemplate = () => {

    const { data: org } = useOrg();

    const queryClient = useQueryClient();

    return useMutation(async (templateDef: ProjectTemplateDefinition) => {
        if (!org) throw Error('No Org');

        const { data } = await api.createProjectTemplate(org.id, {
            body: templateDef,
            onResponse: onResponse
        });

        return data;
    }, {
        onSuccess: data => {
            if (data) {
                queryClient.invalidateQueries(['org', org!.id, 'templates'])
                queryClient.setQueryData(['org', org!.id, 'templates', data!.id], data);
            }
        }
    }).mutateAsync
}

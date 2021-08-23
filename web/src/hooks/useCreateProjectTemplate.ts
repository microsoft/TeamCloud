// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { ProjectTemplateDefinition } from 'teamcloud';
import { api } from '../API';
import { useOrg } from '.';

export const useCreateProjectTemplate = () => {

    const { data: org } = useOrg();

    const queryClient = useQueryClient();

    return useMutation(async (templateDef: ProjectTemplateDefinition) => {
        if (!org) throw Error('No Org');

        const { data } = await api.createProjectTemplate(org.id, {
            body: templateDef,
            onResponse: (raw, flat) => {
                console.warn(JSON.stringify(raw))
                console.warn(JSON.stringify(flat))
                if (raw.status >= 400)
                    throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
            }
        });
        console.warn(JSON.stringify(data))
        return data;
    }, {
        onSuccess: data => {
            console.warn('onsuccess')
            if (data) {
                queryClient.invalidateQueries(['org', org!.id, 'templates'])
                queryClient.setQueryData(['org', org!.id, 'templates', data!.id], data);
            }
        }
    }).mutateAsync
}

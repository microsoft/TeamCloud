// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { DeploymentScopeDefinition } from 'teamcloud';
import { api } from '../API';
import { useOrg } from '.';

export const useCreateDeploymentScope = () => {

    const { data: org } = useOrg();

    const queryClient = useQueryClient();

    return useMutation(async (scopeDef: DeploymentScopeDefinition) => {
        if (!org) throw Error('No Org');

        const { data } = await api.createDeploymentScope(org.id, {
            body: scopeDef,
            onResponse: (raw, flat) => {
                if (raw.status >= 400)
                    throw new Error(raw.parsedBody || raw.bodyAsText || `Error: ${raw.status}`)
            }
        });

        return data;
    }, {
        onSuccess: data => {
            if (data)
                queryClient.setQueryData(['org', org!.id, 'scopes'], [data]);
        }
    }).mutateAsync
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useMutation, useQueryClient } from 'react-query'
import { DeploymentScopeDefinition, ErrorResult } from 'teamcloud';
import { api } from '../API';
import { useOrg } from '.';

export const useCreateDeploymentScope = () => {

    const { data: org } = useOrg();

    const queryClient = useQueryClient();

    return useMutation(async (scopeDef: DeploymentScopeDefinition) => {
        if (!org) throw Error('No Org');

        const { data, code, _response } = await api.createDeploymentScope(org.id, { body: scopeDef });

        if (code && code >= 400) {
            const error = JSON.parse(_response.bodyAsText) as ErrorResult;
            throw error;
        }

        return data;
    }, {
        onSuccess: data => {
            if (data)
                queryClient.setQueryData(['org', org!.id, 'scopes'], [data]);
        }
    }).mutateAsync
}

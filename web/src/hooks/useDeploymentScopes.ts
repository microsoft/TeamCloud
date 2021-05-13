// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useOrg } from '.';
import { ErrorResult } from 'teamcloud';

export const useDeploymentScopes = () => {

    const { data: org } = useOrg();

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['org', org?.id, 'scopes'], async () => {

        const { data, code, _response } = await api.getDeploymentScopes(org!.id);

        if (code && code >= 400) {
            const error = JSON.parse(_response.bodyAsText) as ErrorResult;
            throw error;
        }

        return data;
    }, {
        enabled: isAuthenticated && !!org?.id
    });
}

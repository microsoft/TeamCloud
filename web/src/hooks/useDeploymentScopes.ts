// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useOrg } from '.';

export const useDeploymentScopes = () => {

    const { data: org } = useOrg();

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['org', org?.id, 'scopes'], async () => {
        const { data } = await api.getDeploymentScopes(org!.id);
        return data;
    }, {
        enabled: isAuthenticated && !!org?.id
    });
}

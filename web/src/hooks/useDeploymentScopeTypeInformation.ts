// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useOrg } from '.';

export const useDeploymentScopeTypeInformation = () => {

    const { data: org } = useOrg();

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['org', org?.id, 'scopes', 'types'], async () => {
        const { data } = await api.getDeploymentScopeTypeInformation(org!.id);
        return data;
    }, {
        enabled: isAuthenticated && !!org?.id
    });
}

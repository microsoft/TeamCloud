// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useOrg } from '.';

export const useUser = () => {

    const { data: org } = useOrg();

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['org', org?.id, 'user', 'me'], async () => {
        const { data } = await api.getOrganizationUserMe(org!.id);
        return data;
    }, {
        enabled: isAuthenticated && !!org?.id
    });
}

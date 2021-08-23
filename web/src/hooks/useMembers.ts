// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { Member } from '../model';
import { useOrg, useUsers } from '.';
import { getGraphPrincipal } from '../MSGraph';

export const useMembers = () => {

    const isAuthenticated = useIsAuthenticated();

    const { data: org } = useOrg();
    const { data: users } = useUsers();

    return useQuery(['org', org?.id, 'members'],
        async () => await Promise.all(users!.map(async u => new Member(u, await getGraphPrincipal(u)))),
        {
            enabled: isAuthenticated && !!org?.id && !!users && users.length > 0
        });
}

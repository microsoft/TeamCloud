// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { getGraphUser } from '../MSGraph';
import { api } from '../API';
import { useOrg } from '.';

export const useMembers = () => {

    const { data: org } = useOrg();

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['org', org?.id, 'members'], async () => {
        let _users = await api.getOrganizationUsers(org!.id);
        if (_users.data) {
            let _members = await Promise.all(_users.data.map(async u => ({
                user: u,
                graphUser: await getGraphUser(u.id)
            })));
            return _members;
        }
        return [];
    }, {
        enabled: isAuthenticated && !!org?.id
    });
}

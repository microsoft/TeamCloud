// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { Member } from '../model';
import { useOrg, useUser, useGraphUser } from '.';

export const useMember = () => {

    const isAuthenticated = useIsAuthenticated();

    const { data: org } = useOrg();
    const { data: user } = useUser();
    const { data: graphUser } = useGraphUser();

    return useQuery(['org', org?.id, 'members', 'me'],
        () => new Member(user!, graphUser!),
        {
            enabled: isAuthenticated && !!org?.id && !!user && !!graphUser
        });
}

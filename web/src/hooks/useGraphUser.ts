// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { getMe } from '../MSGraph';

export const useGraphUser = () => {

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['graphUser', 'me'], async () => await getMe(), {
        refetchOnMount: false,
        refetchOnWindowFocus: false,
        staleTime: 1000 * 60 * 5,
        enabled: isAuthenticated
    });
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { getSubscriptions } from '../Azure';

export const useAzureSubscriptions = () => {

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['azure', 'subscriptions'], async () => {
        try {
            const subs = await getSubscriptions();
            return subs ?? [];
        } catch (error) {
            console.error(error);
            return [];
        }
    }, {
        enabled: isAuthenticated
    });
}

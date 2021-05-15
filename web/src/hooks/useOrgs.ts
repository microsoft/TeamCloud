// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { ErrorResult } from 'teamcloud';

export const useOrgs = () => {

    const isAuthenticated = useIsAuthenticated();

    return useQuery('orgs', async () => {

        const { data, code, _response } = await api.getOrganizations();

        if (code && code >= 400) {
            const error = JSON.parse(_response.bodyAsText) as ErrorResult;
            throw error;
        }

        return data
    }, {
        enabled: isAuthenticated
    });
}

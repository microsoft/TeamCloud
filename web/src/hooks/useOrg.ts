// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useParams } from 'react-router-dom';
import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { ErrorResult } from 'teamcloud';

export const useOrg = () => {

    const { orgId } = useParams() as { orgId: string };

    const isAuthenticated = useIsAuthenticated();

    return useQuery(['org', orgId], async () => {

        const { data, code, _response } = await api.getOrganization(orgId);

        if (code && code >= 400) {
            const error = JSON.parse(_response.bodyAsText) as ErrorResult;
            throw error;
        }

        return data;
    }, {
        enabled: isAuthenticated && !!orgId
    });
}

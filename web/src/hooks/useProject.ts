// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useParams } from 'react-router-dom';
import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useOrg } from '.';
import { ErrorResult } from 'teamcloud';

export const useProject = () => {

    const { projectId } = useParams() as { projectId: string };

    const isAuthenticated = useIsAuthenticated();

    const { data: org } = useOrg();

    return useQuery(['org', org?.id, 'project', projectId], async () => {

        const { data, code, _response } = await api.getProject(projectId, org!.id);

        if (code && code >= 400) {
            const error = JSON.parse(_response.bodyAsText) as ErrorResult;
            throw error;
        }

        return data;
    }, {
        enabled: isAuthenticated && !!org?.id && !!projectId
    });
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useProject } from '.';
import { ErrorResult } from 'teamcloud';

export const useProjectComponentTemplates = () => {

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();

    return useQuery(['org', project?.organization, 'project', project?.id, 'componenttemplate'], async () => {

        const { data, code, _response } = await api.getComponentTemplates(project!.organization, project!.id);

        if (code && code >= 400) {
            const error = JSON.parse(_response.bodyAsText) as ErrorResult;
            throw error;
        }

        return data;
    }, {
        enabled: isAuthenticated && !!project?.id
    });
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { api } from '../API';
import { useProject } from '.';

export const useProjectComponent = () => {

    const { itemId } = useParams() as { itemId: string };

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();

    return useQuery(['org', project?.organization, 'project', project?.id, 'component', itemId], async () => {
        const { data } = await api.getComponent(itemId, project!.organization, project!.id)
        return data;
    }, {
        enabled: isAuthenticated && !!project?.id && !!itemId
    });
}

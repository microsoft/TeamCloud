// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { ProjectMember } from '../model';
import { getGraphPrincipal } from '../MSGraph';
import { useProject, useProjectUsers } from '.';

export const useProjectMembers = () => {

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();
    const { data: users } = useProjectUsers();

    return useQuery(['org', project?.organization, 'project', project?.id, 'members'],
        async () => await Promise.all(users!.map(async u => new ProjectMember(u, await getGraphPrincipal(u), project!.id))),
        {
            enabled: isAuthenticated && !!project?.id && !!users && users.length > 0
        });
}

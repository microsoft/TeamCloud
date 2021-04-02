// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { getGraphUser } from '../MSGraph';
import { api } from '../API';
import { useProject } from '.';

export const useProjectMembers = () => {

    const isAuthenticated = useIsAuthenticated();

    const { data: project } = useProject();

    return useQuery(['org', project?.organization, 'project', project?.id, 'user'], async () => {
        let _users = await api.getProjectUsers(project!.organization, project!.id);
        if (_users.data) {
            let _members = await Promise.all(_users.data.map(async u => ({
                user: u,
                graphUser: await getGraphUser(u.id),
                projectMembership: u.projectMemberships!.find(m => m.projectId === project!.id)!
            })));
            return _members;
        }
        return [];
    }, {
        enabled: isAuthenticated && !!project?.id
    });
}

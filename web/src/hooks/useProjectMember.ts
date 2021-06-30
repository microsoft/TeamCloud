// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { useQuery } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { useProject, useProjectMembers, useUser } from '.';

export const useProjectMember = () => {

    const isAuthenticated = useIsAuthenticated();

    const { data: user } = useUser();
    const { data: project } = useProject();
    const { data: members } = useProjectMembers();

    return useQuery(['org', project?.organization, 'project', project?.id, 'members', 'me'],
        () => members?.find(m => m.user.id === user?.id),
        {
            enabled: isAuthenticated && !!project?.id && !!user?.id && !!members && members.length > 0
        });
}

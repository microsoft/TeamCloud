// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from 'react-query'
import { useIsAuthenticated } from '@azure/msal-react';
import { DeploymentScopeDefinition, ProjectDefinition, ProjectTemplateDefinition, UserDefinition } from 'teamcloud';
import { OrgContext } from '../Context';
import { getGraphUser } from '../MSGraph';
import { api } from '../API';

export const OrgProvider = (props: any) => {

    const history = useHistory();

    const { orgId } = useParams() as { orgId: string };

    const isAuthenticated = useIsAuthenticated();

    const queryClient = useQueryClient();


    const createDeploymentScope = useMutation(async (scopeDef: DeploymentScopeDefinition) => {
        if (!org) throw Error('No Org');

        const response = await api.createDeploymentScope(org.id, { body: scopeDef });
        return response.data;
    }, {
        onSuccess: data => {
            if (data)
                queryClient.setQueryData(['org', org!.id, 'scopes'], [data]);
        }
    });


    const createProjectTemplate = useMutation(async (templateDef: ProjectTemplateDefinition) => {
        if (!org) throw Error('No Org');

        const response = await api.createProjectTemplate(org.id, { body: templateDef });
        return response.data;
    }, {
        onSuccess: data => {
            if (data)
                queryClient.setQueryData(['org', org!.id, 'templates'], [data]);
        }
    });


    const createProject = useMutation(async (projectDef: ProjectDefinition) => {
        if (!org) throw Error('No Org');

        const response = await api.createProject(org.id, { body: projectDef });
        return response.data;
    }, {
        onSuccess: data => {
            if (data && org) {
                queryClient.setQueryData(['org', org.id, 'project', data.slug], data);
                queryClient.setQueryData(['org', org.id, 'projects'], projects ? [...projects, data] : [data]);

                history.push(`/orgs/${org.slug}/projects/${data.slug}`);
            }
        }
    })


    const addUsers = useMutation(async (users: UserDefinition[]) => {
        if (!org) throw Error('No Org');

        const responses = await Promise
            .all(users.map(async d => await api.createOrganizationUser(org.id, { body: d })));

        responses.forEach(r => {
            if (!r.data)
                console.error(r);
        });

        const newMembers = await Promise.all(responses
            .filter(r => r.data)
            .map(r => r.data!)
            .map(async u => ({
                user: u,
                graphUser: await getGraphUser(u.id)
            })));

        return newMembers;
    }, {
        onSuccess: data => {
            queryClient.setQueryData(['org', org!.id, 'members'], members ? [...members, ...data] : data);
        }
    });


    // Org (selected)
    const { data: org } = useQuery(['org', orgId], async () => {
        const response = await api.getOrganization(orgId);
        return response.data;
    }, {
        enabled: isAuthenticated && !!orgId
    });


    // Current User
    const { data: user } = useQuery(['org', org?.id, 'user', 'me'], async () => {
        const response = await api.getOrganizationUserMe(org!.id);
        return response.data;
    }, {
        enabled: isAuthenticated && !!org?.id
    });


    // Projects
    const { data: projects } = useQuery(['org', org?.id, 'projects'], async () => {
        const response = await api.getProjects(org!.id);
        return response.data;
    }, {
        enabled: isAuthenticated && !!org?.id
    });


    // Members
    const { data: members } = useQuery(['org', org?.id, 'members'], async () => {
        let _users = await api.getOrganizationUsers(org!.id);
        if (_users.data) {
            let _members = await Promise.all(_users.data.map(async u => ({
                user: u,
                graphUser: await getGraphUser(u.id)
            })));
            return _members;
        }
        return [];
    }, {
        enabled: isAuthenticated && !!org?.id
    });


    // Deployment Scopes
    const { data: scopes } = useQuery(['org', org?.id, 'scopes'], async () => {
        const response = await api.getDeploymentScopes(org!.id);
        return response.data;
    }, {
        enabled: isAuthenticated && !!org?.id
    });


    // Project Templates
    const { data: templates } = useQuery(['org', org?.id, 'templates'], async () => {
        const response = await api.getProjectTemplates(org!.id);
        return response.data;
    }, {
        enabled: isAuthenticated && !!org?.id
    });

    return <OrgContext.Provider value={{
        org: org,
        user: user,
        scopes: scopes,
        members: members,
        projects: projects,
        templates: templates,
        addUsers: addUsers.mutateAsync,
        removeUsers: () => Promise.resolve(),
        createProject: createProject.mutateAsync,
        createDeploymentScope: createDeploymentScope.mutateAsync,
        createProjectTemplate: createProjectTemplate.mutateAsync
    }} {...props} />
}

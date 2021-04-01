// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useCallback, useEffect } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from 'react-query';
import { useIsAuthenticated } from '@azure/msal-react';
import { ComponentDefinition, ComponentTaskDefinition, UserDefinition } from 'teamcloud';
import { getGraphUser } from '../MSGraph';
import { ProjectContext } from '../Context';
import { matchesLowerCase, matchesRouteParam } from '../Utils';
import { api, resolveSignalR } from '../API';
import { useOrg } from '../Hooks';
import { Message } from '../model';

export const ProjectProvider = (props: any) => {

    const history = useHistory();

    const { orgId, projectId, navId, itemId, subitemId } = useParams() as { orgId: string, projectId: string, navId: string, itemId: string, subitemId: string };

    const isAuthenticated = useIsAuthenticated();

    const queryClient = useQueryClient();

    const { org } = useOrg()


    const { data: project } = useQuery(['org', org?.id, 'project', projectId], async () => {
        const response = await api.getProject(projectId, org!.id);
        return response.data;
    }, {
        enabled: isAuthenticated && !!org?.id && !!projectId
    });


    const handleMessage = useCallback((action: string, data: any) => {

        const message = data as Message;

        if (!message)
            throw Error('Message is not in the correct format');

        let typeQueries: [string[]] = [[]];
        let itemQueries: [string[]] = [[]];

        message.items.forEach(item => {

            if (!item.organization || !item.project || !item.type || !item.id)
                throw Error('Missing required stuff');

            let queryId = ['org', item.organization, 'project', item.project];

            if (item.component)
                queryId.push('component', item.component);

            queryId.push(item.type);

            if (!typeQueries.includes(queryId))
                typeQueries.push(queryId);

            queryId.push(item.id);

            if (!itemQueries.includes(queryId))
                itemQueries.push(queryId);
        });

        switch (action) {
            case 'create':
                typeQueries.forEach(q => queryClient.invalidateQueries(q));
                break;
            case 'update':
                itemQueries.forEach(q => queryClient.invalidateQueries(q));
                typeQueries.forEach(q => queryClient.invalidateQueries(q));
                break;
            case 'delete':
                itemQueries.forEach(q => queryClient.removeQueries(q));
                typeQueries.forEach(q => queryClient.invalidateQueries(q));
                break;
            case 'custom':
                console.log(`$ unhandled ${action}: ${data}`);
                break;
            default:
                console.log(`$ unhandled ${action}: ${data}`);
                break;
        }
    }, [queryClient]);


    useEffect(() => {
        const _resolve = async () => {
            try {
                await resolveSignalR(project, handleMessage);
            } catch (error) {
                console.error(error);
            }
        }
        _resolve();
    }, [project, handleMessage]);


    const addUsers = useMutation(async (users: UserDefinition[]) => {
        if (!project) throw Error('No project')

        const responses = await Promise
            .all(users.map(async d => await api.createProjectUser(project.organization, project.id, { body: d })));

        responses.forEach(r => {
            if (!r.data)
                console.error(r);
        });

        const newMembers = await Promise.all(responses
            .filter(r => r.data)
            .map(r => r.data!)
            .map(async u => ({
                user: u,
                graphUser: await getGraphUser(u.id),
                projectMembership: u.projectMemberships!.find(m => m.projectId === project.id)!
            })));

        return newMembers;
    }, {
        onSuccess: data => {
            queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'user'], members ? [...members, ...data] : data)
        }
    });


    const createComponent = useMutation(async (componentDef: ComponentDefinition) => {
        if (!project) throw Error('No project')

        const response = await api.createComponent(project.organization, project.id, { body: componentDef });
        return response.data
    }, {
        onSuccess: data => {
            if (data) {
                queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'component', data.slug], data)
                queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'component'], components ? [...components, data] : [data])

                history.push(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/components/${data?.slug}`);
            }
        }
    });


    const createComponentTask = useMutation(async (componentTaskDef: ComponentTaskDefinition) => {
        if (!project) throw Error('No project')
        if (!component) throw Error('No component')

        const response = await api.createComponentTask(project.organization, project.id, component.id, { body: componentTaskDef });
        return response.data
    }, {
        onSuccess: data => {
            if (data) {
                queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask', data.id], data)
                queryClient.setQueryData(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask'], componentTasks ? [...componentTasks, data] : [data])

                history.push(`/orgs/${org?.slug ?? orgId}/projects/${project?.slug ?? projectId}/components/${component?.slug ?? itemId}/tasks/${data.id}`);
            }
        }
    });


    // Project Members
    const { data: members } = useQuery(['org', project?.organization, 'project', project?.id, 'user'], async () => {
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


    // Component (selected)
    const { data: component } = useQuery(['org', project?.organization, 'project', project?.id, 'component', itemId], async () => {
        const response = await api.getComponent(itemId, project!.organization, project!.id)
        return response.data;
    }, {
        enabled: isAuthenticated && !!project?.id && !!navId && matchesLowerCase(navId, 'components') && !!itemId
    });


    // Project Components
    const { data: components } = useQuery(['org', project?.organization, 'project', project?.id, 'component'], async () => {
        const response = await api.getComponents(project!.organization, project!.id);
        return response.data;
    }, {
        enabled: isAuthenticated && !!project?.id
    });


    // Project Component Tasks
    const { data: componentTasks } = useQuery(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask'], async () => {
        const response = await api.getComponentTasks(project!.organization, project!.id, component!.id);
        return response.data;
    }, {
        enabled: isAuthenticated && !!project?.id && !!component?.id && !!navId && matchesLowerCase(navId, 'components') && !!itemId && matchesRouteParam(component, itemId)
    });


    // Project Component Task (selected)
    const { data: projectComponentTask2 } = useQuery(['org', project?.organization, 'project', project?.id, 'component', component?.id, 'componenttask', subitemId], async () => {
        const response = await api.getComponentTask(subitemId, project!.organization, project!.id, component!.id);
        return response.data;
    }, {
        enabled: isAuthenticated && !!project?.id && !!navId && matchesLowerCase(navId, 'components') && !!component?.id && !!itemId && matchesRouteParam(component, itemId) && !!subitemId
    });


    // Project Component Templates
    const { data: componentTemplates } = useQuery(['org', project?.organization, 'project', project?.id, 'componenttemplate'], async () => {
        const response = await api.getComponentTemplates(project!.organization, project!.id);
        return response.data;
    }, {
        enabled: isAuthenticated && !!project?.id
    });


    return <ProjectContext.Provider value={{
        project: project,
        members: members,
        component: component,
        components: components,
        templates: componentTemplates,
        componentTask: projectComponentTask2,
        componentTasks: componentTasks,
        addUsers: addUsers.mutateAsync,
        createComponent: createComponent.mutateAsync,
        createComponentTask: createComponentTask.mutateAsync,
    }} {...props} />
}

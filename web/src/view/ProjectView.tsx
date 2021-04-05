// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useCallback, useEffect, useState } from 'react';
import { Route, Switch } from 'react-router-dom';
import { Stack, IconButton } from '@fluentui/react';
import { useQueryClient } from 'react-query';
import { ComponentTaskMenu, ComponentOverview, ProjectOverview, ContentHeader, ContentProgress, ContentContainer, MemberList, ComponentList, ComponentForm, ProjectSettingsOverview } from '../components';
import { useAddProjectMembers, useProject, useProjectComponent, useProjectComponents, useProjectComponentTemplates, useProjectMembers } from '../hooks';
import { startSignalR, stopSignalR } from '../API';
import { Message } from '../model';
import { Component } from 'teamcloud';

export const ProjectView: React.FC = () => {

    const [favorite, setFavorate] = useState(false);

    const { data: component } = useProjectComponent();

    const { data: project, isLoading: projectIsLoading } = useProject();
    const { data: members, isLoading: membersIsLoading } = useProjectMembers();
    // const { data: components } = useProjectComponents();

    const { isLoading: componentsIsLoading } = useProjectComponents();
    const { isLoading: templatesIsLoading } = useProjectComponentTemplates();

    const queryClient = useQueryClient();

    const addMembers = useAddProjectMembers();

    const handleMessage = useCallback((action: string, data: any) => {

        const message = data as Message;

        if (!message)
            throw Error('Message is not in the correct format');

        let typeQueries: string[][] = [];
        let itemQueries: string[][] = [];

        message.items.forEach(item => {

            if (!item.organization || !item.project || !item.type || !item.id)
                throw Error('Missing required stuff');

            let queryId = ['org', item.organization, 'project', item.project];

            if (item.component)
                queryId.push('component', item.component);

            queryId.push(item.type);

            if (!typeQueries.includes(queryId))
                typeQueries.push(queryId);

            if (item.type === 'component') {
                const components: Component[] | undefined = queryClient.getQueryData(['org', item?.organization, 'project', item.project, 'component'])
                queryId.push(components?.find(c => c.id === item.id)?.slug ?? item.id);
            } else {
                queryId.push(item.id);
            }

            if (!itemQueries.includes(queryId))
                itemQueries.push(queryId);

            if (item.type === 'componenttask') {
                queryId.push('poll')
                if (!itemQueries.includes(queryId))
                    itemQueries.push(queryId);
            }
        });

        switch (action) {
            case 'create':
                typeQueries.forEach(q => {
                    console.log(`create: invalidating query: ${q}`);
                    queryClient.invalidateQueries(q, { exact: true });
                });
                itemQueries.forEach(q => {
                    console.log(`create: invalidating query: ${q}`);
                    queryClient.invalidateQueries(q, { exact: true });
                });
                // typeQueries.forEach(q => queryClient.invalidateQueries(q, { exact: true }));
                break;
            case 'update':
                itemQueries.forEach(q => {
                    console.log(`update: invalidating query: ${q}`);
                    queryClient.invalidateQueries(q, { exact: true });
                });
                typeQueries.forEach(q => {
                    console.log(`update: invalidating query: ${q}`);
                    queryClient.invalidateQueries(q, { exact: true })
                });
                // itemQueries.forEach(q => queryClient.invalidateQueries(q, { exact: true }));
                // typeQueries.forEach(q => queryClient.invalidateQueries(q, { exact: true }));
                break;
            case 'delete':
                itemQueries.forEach(q => {
                    if (q)
                        console.log(`delete: removing query: ${q}`);
                    queryClient.removeQueries(q, { exact: true })
                });
                typeQueries.forEach(q => {
                    console.log(`delete: invalidating query: ${q}`);
                    queryClient.invalidateQueries(q, { exact: true })
                });
                // itemQueries.forEach(q => queryClient.removeQueries(q, { exact: true }));
                // typeQueries.forEach(q => queryClient.invalidateQueries(q, { exact: true }));
                break;
            default:
                console.log(`$ unhandled ${action}: ${data}`);
                break;
        }
    }, [queryClient]);


    useEffect(() => {
        if (project) {
            try {
                startSignalR(project, handleMessage);
            } catch (error) {
                console.error(error);
            }
        }

        return () => {
            if (project) {
                try {
                    stopSignalR();
                } catch (error) {
                    console.error(error);
                }
            }
        }
    }, [project, handleMessage])




    return (
        <Stack>
            <Switch>
                <Route exact path='/orgs/:orgId/projects/:projectId'>
                    <ContentProgress progressHidden={!projectIsLoading && !componentsIsLoading && !membersIsLoading} />
                    <ContentHeader title={project?.displayName} coin>
                        <IconButton toggle checked={favorite} onClick={() => setFavorate(!favorite)}
                            iconProps={{ iconName: favorite ? 'FavoriteStarFill' : 'FavoriteStar', color: 'yellow' }} />
                    </ContentHeader>
                    <ContentContainer>
                        <ProjectOverview />
                    </ContentContainer>
                </Route>
                <Route exact path='/orgs/:orgId/projects/:projectId/components/new'>
                    <ComponentForm />
                </Route>
                <Route exact path='/orgs/:orgId/projects/:projectId/components'>
                    <ContentProgress progressHidden={!projectIsLoading && !componentsIsLoading && !templatesIsLoading && !membersIsLoading} />
                    <ContentHeader title='Components' />
                    <ContentContainer>
                        <ComponentList />
                    </ContentContainer>
                </Route>
                <Route exact path={['/orgs/:orgId/projects/:projectId/components/:itemId', '/orgs/:orgId/projects/:projectId/components/:itemId/tasks/:subitemId']}>
                    <ContentProgress progressHidden={!projectIsLoading && !componentsIsLoading && !templatesIsLoading && !membersIsLoading} />
                    <ContentHeader title={component?.displayName ?? undefined}>
                        <ComponentTaskMenu />
                    </ContentHeader>
                    <ContentContainer>
                        <ComponentOverview />
                    </ContentContainer>
                </Route>
                <Route exact path='/orgs/:orgId/projects/:projectId/members'>
                    <ContentProgress progressHidden={!projectIsLoading && !membersIsLoading} />
                    <ContentHeader title='Members' />
                    <ContentContainer>
                        <MemberList {...{ project: project, members: members, addMembers: addMembers }} />
                    </ContentContainer>
                </Route>
                <Route exact path='/orgs/:orgId/projects/:projectId/settings'>
                    <ContentProgress progressHidden={!projectIsLoading && !membersIsLoading} />
                    <ContentHeader title={`${(project?.displayName ? (project.displayName + ' - Settings') : 'Settings')}`} coin={project?.displayName !== undefined} />
                    <ContentContainer>
                        <ProjectSettingsOverview />
                    </ContentContainer>
                </Route>
            </Switch>
        </Stack>
    );
}

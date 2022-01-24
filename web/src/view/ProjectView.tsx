// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useCallback, useEffect, useState } from 'react';
import { Route, Routes, useNavigate, useParams } from 'react-router-dom';
import { Stack, IconButton } from '@fluentui/react';
import { useQueryClient } from 'react-query';
import { ComponentTaskMenu, ComponentOverview, ProjectOverview, ContentHeader, ContentProgress, ContentContainer, MemberList, ComponentList, ComponentForm, ProjectSettingsOverview, ScheduleForm, ScheduleList } from '../components';
import { useAddProjectMembers, useOrg, useProject, useProjectComponent, useProjectComponents, useProjectComponentTemplates, useProjectMembers, useProjectSchedule, useProjectSchedules } from '../hooks';
import { startSignalR, stopSignalR } from '../API';
import { Message } from '../model';

export const ProjectView: React.FC = () => {

    const navigate = useNavigate();
    const { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const [favorite, setFavorate] = useState(false);

    const { data: component } = useProjectComponent();

    const { isLoading: orgIsLoading } = useOrg();

    const { data: project, isLoading: projectIsLoading } = useProject();
    const { data: members, isLoading: membersIsLoading } = useProjectMembers();
    // const { data: components } = useProjectComponents();

    const { isLoading: componentsIsLoading } = useProjectComponents();
    const { isLoading: templatesIsLoading } = useProjectComponentTemplates();
    const { isLoading: schedulesIsLoading } = useProjectSchedules();
    const { isLoading: scheduleIsLoading } = useProjectSchedule();

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
                typeQueries.push([...queryId]);

            if (item.slug) {
                const slugQueryId = [...queryId, item.slug];
                if (!itemQueries.includes(slugQueryId))
                    itemQueries.push(slugQueryId);
            }

            queryId.push(item.id);

            if (!itemQueries.includes(queryId))
                itemQueries.push(queryId);
        });

        switch (action) {
            case 'create':
                typeQueries.forEach(q => queryClient.invalidateQueries(q, { exact: true }));
                itemQueries.forEach(q => queryClient.invalidateQueries(q, { exact: true }));
                break;
            case 'update':
                itemQueries.forEach(q => queryClient.invalidateQueries(q, { exact: true }));
                typeQueries.forEach(q => queryClient.invalidateQueries(q, { exact: true }));
                break;
            case 'delete':
                itemQueries.forEach(q => queryClient.removeQueries(q, { exact: true }));
                typeQueries.forEach(q => queryClient.invalidateQueries(q, { exact: true }));
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
        <Routes>
            <Route path='' element={
                <Stack>
                    <ContentProgress progressHidden={!orgIsLoading && !projectIsLoading && !componentsIsLoading && !membersIsLoading} />
                    <ContentHeader title={project?.displayName} coin>
                        <IconButton toggle checked={favorite} onClick={() => setFavorate(!favorite)}
                            iconProps={{ iconName: favorite ? 'FavoriteStarFill' : 'FavoriteStar', color: 'yellow' }} />
                    </ContentHeader>
                    <ContentContainer>
                        <ProjectOverview />
                    </ContentContainer>
                </Stack>
            } />
            <Route path='components/new' element={<Stack><ComponentForm /></Stack>} />

            <Route path='components' element={
                <Stack>
                    <ContentProgress progressHidden={!orgIsLoading && !projectIsLoading && !componentsIsLoading && !templatesIsLoading && !membersIsLoading} />
                    <ContentHeader title='Components' />
                    <ContentContainer>
                        <ComponentList />
                    </ContentContainer>
                </Stack>
            } />

            <Route path='components/:itemId/*' element={
                <Stack>
                    <ContentProgress progressHidden={!orgIsLoading && !projectIsLoading && !componentsIsLoading && !templatesIsLoading && !membersIsLoading} />
                    <ContentHeader title={component?.displayName ?? undefined}>
                        <ComponentTaskMenu />
                    </ContentHeader>
                    <ContentContainer>
                        <ComponentOverview />
                    </ContentContainer>
                </Stack>
            } />

            <Route path='members' element={
                <Stack>
                    <ContentProgress progressHidden={!orgIsLoading && !projectIsLoading && !membersIsLoading} />
                    <ContentHeader title='Members' />
                    <ContentContainer>
                        <MemberList {...{ project: project, members: members, addMembers: addMembers }} />
                    </ContentContainer>
                </Stack>
            } />

            <Route path='settings' element={
                <Stack>
                    <ContentProgress progressHidden={!orgIsLoading && !projectIsLoading && !membersIsLoading} />
                    <ContentHeader title={`${(project?.displayName ? (project.displayName + ' - Settings') : 'Settings')}`} coin={project?.displayName !== undefined} />
                    <ContentContainer>
                        <ProjectSettingsOverview />
                    </ContentContainer>
                </Stack>
            } />

            <Route path='settings/schedules' element={
                <Stack>
                    <ContentProgress progressHidden={!orgIsLoading && !projectIsLoading && !schedulesIsLoading} />
                    <ContentHeader title={`${(project?.displayName ? (project.displayName + ' - Schedules') : 'Schedules')}`} coin={project?.displayName !== undefined} />
                    <ContentContainer>
                        <ScheduleList />
                    </ContentContainer>
                </Stack>
            } />

            <Route path='settings/schedules/new' element={
                <Stack>
                    <ContentProgress progressHidden={!orgIsLoading && !projectIsLoading && !membersIsLoading && !componentsIsLoading && !templatesIsLoading} />
                    <ContentHeader title={`${(project?.displayName ? (project.displayName + ' - New Schedule') : 'New Schedule')}`} coin={project?.displayName !== undefined} >
                        <IconButton iconProps={{ iconName: 'ChromeClose' }}
                            onClick={() => navigate(`/orgs/${orgId}/projects/${projectId}/settings/schedules`)} />
                    </ContentHeader>
                    <ContentContainer>
                        <ScheduleForm />
                    </ContentContainer>
                </Stack>
            } />

            <Route path='settings/schedules/:itemId' element={
                <Stack>
                    <ContentProgress progressHidden={!orgIsLoading && !projectIsLoading && !membersIsLoading && !componentsIsLoading && !templatesIsLoading && !scheduleIsLoading} />
                    <ContentHeader title={`${(project?.displayName ? (project.displayName + ' - Schedule') : 'Schedule')}`} coin={project?.displayName !== undefined} >
                        <IconButton iconProps={{ iconName: 'ChromeClose' }}
                            onClick={() => navigate(`/orgs/${orgId}/projects/${projectId}/settings/schedules`)} />
                    </ContentHeader>
                    <ContentContainer>
                        <ScheduleForm />
                    </ContentContainer>
                </Stack>
            } />
        </Routes>
    );
}

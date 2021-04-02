// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Route, Switch } from 'react-router-dom';
import { Stack, IconButton } from '@fluentui/react';
import { ComponentOverview, ProjectOverview, ContentHeader, ContentProgress, ContentContainer, MemberList, ComponentList, ComponentForm, ProjectSettingsOverview } from '../components';
import { ComponentTaskMenu } from '../components/ComponentTaskMenu';
import { useAddProjectMembers, useProject, useProjectComponent, useProjectComponents, useProjectComponentTemplates, useProjectMembers } from '../hooks';

export const ProjectView: React.FC = () => {

    const [favorite, setFavorate] = useState(false);

    const { data: component } = useProjectComponent();

    const { data: project, isLoading: projectIsLoading } = useProject();
    const { data: members, isLoading: membersIsLoading } = useProjectMembers();

    const { isLoading: componentsIsLoading } = useProjectComponents();
    const { isLoading: templatesIsLoading } = useProjectComponentTemplates();

    const addMembers = useAddProjectMembers();

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

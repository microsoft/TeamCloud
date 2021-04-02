// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Route, Switch } from 'react-router-dom';
import { Stack, IconButton } from '@fluentui/react';
import { ComponentOverview, ProjectOverview, ContentHeader, ContentProgress, ContentContainer, MemberList, ComponentList, ComponentForm, ProjectSettingsOverview } from '../components';
import { ComponentTaskMenu } from '../components/ComponentTaskMenu';
import { useProject } from '../Hooks';

export const ProjectView: React.FC = () => {

    const [favorite, setFavorate] = useState(false);

    const { project, members, components, component, templates, addUsers } = useProject();

    return (
        <Stack>
            <Switch>
                <Route exact path='/orgs/:orgId/projects/:projectId'>
                    <ContentProgress progressHidden={project !== undefined && components !== undefined && members !== undefined} />
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
                    <ContentProgress progressHidden={project !== undefined && components !== undefined && templates !== undefined && members !== undefined} />
                    <ContentHeader title='Components' />
                    <ContentContainer>
                        <ComponentList />
                    </ContentContainer>
                </Route>
                <Route exact path={['/orgs/:orgId/projects/:projectId/components/:itemId', '/orgs/:orgId/projects/:projectId/components/:itemId/tasks/:subitemId']}>
                    <ContentProgress progressHidden={project !== undefined && components !== undefined && templates !== undefined && members !== undefined} />
                    <ContentHeader title={component?.displayName ?? undefined}>
                        <ComponentTaskMenu />
                    </ContentHeader>
                    <ContentContainer>
                        <ComponentOverview />
                    </ContentContainer>
                </Route>
                <Route exact path='/orgs/:orgId/projects/:projectId/members'>
                    <ContentProgress progressHidden={project !== undefined && members !== undefined} />
                    <ContentHeader title='Members' />
                    <ContentContainer>
                        <MemberList {...{ project: project, members: members, addUsers: addUsers }} />
                    </ContentContainer>
                </Route>
                <Route exact path='/orgs/:orgId/projects/:projectId/settings'>
                    <ContentProgress progressHidden={project !== undefined && members !== undefined} />
                    <ContentHeader title={`${(project?.displayName ? (project.displayName + ' - Settings') : 'Settings')}`} coin={project?.displayName !== undefined} />
                    <ContentContainer>
                        <ProjectSettingsOverview />
                    </ContentContainer>
                </Route>
            </Switch>
        </Stack>
    );
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useContext } from 'react';
import { Route, Switch } from 'react-router-dom';
import { Stack, IconButton } from '@fluentui/react';
import { ComponentOverview, ProjectOverview, ContentHeader, ContentProgress, ContentContainer, MemberList, ComponentList, ComponentForm, ProjectSettingsOverview } from '../components';
import { ProjectContext } from '../Context';

export const ProjectView: React.FC = () => {

    const [favorite, setFavorate] = useState(false);

    const { project, members, components, component, templates, onAddUsers } = useContext(ProjectContext)

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
                <Route exact path='/orgs/:orgId/projects/:projectId/components/:itemId'>
                    <ContentProgress progressHidden={project !== undefined && components !== undefined && templates !== undefined && members !== undefined} />
                    <ContentHeader title={component?.displayName ?? undefined} />
                    <ContentContainer>
                        <ComponentOverview />
                    </ContentContainer>
                </Route>
                <Route exact path='/orgs/:orgId/projects/:projectId/members'>
                    <ContentProgress progressHidden={project !== undefined && members !== undefined} />
                    <ContentHeader title='Members' />
                    <ContentContainer>
                        <MemberList {...{ project: project, members: members, onAddUsers: onAddUsers }} />
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

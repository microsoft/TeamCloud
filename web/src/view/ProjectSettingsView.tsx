// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Route } from 'react-router-dom';
import { Stack, IconButton } from '@fluentui/react';
import { Project, User } from 'teamcloud';
import { ProjectSettingsOverview, ContentHeader, ContentContainer, ContentProgress } from '../components';

export interface IProjectSettingsViewProps {
    user?: User;
    project?: Project;
}

export const ProjectSettingsView: React.FC<IProjectSettingsViewProps> = (props) => {

    const [favorite, setFavorate] = useState(false);

    return (
        <Stack>
            <ContentProgress progressHidden={props.project !== undefined} />
            <ContentHeader title={props.project?.displayName}>
                <IconButton
                    toggle
                    checked={favorite}
                    onClick={() => setFavorate(!favorite)}
                    iconProps={{ iconName: favorite ? 'FavoriteStarFill' : 'FavoriteStar', color: 'yellow' }} />
            </ContentHeader>
            <ContentContainer>
                <Route exact path='/orgs/:orgId/projects/:projectId/settings'>
                    <ProjectSettingsOverview {...{ project: props.project, user: props.user }} />
                </Route>
            </ContentContainer>
        </Stack>
    );
}

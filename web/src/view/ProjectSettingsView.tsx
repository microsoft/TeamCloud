// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useContext, useState } from 'react';
import { Route } from 'react-router-dom';
import { Stack, IconButton } from '@fluentui/react';
import { ProjectSettingsOverview, ContentHeader, ContentContainer, ContentProgress } from '../components';
import { OrgContext } from '../Context';

export const ProjectSettingsView: React.FC = () => {

    const [favorite, setFavorate] = useState(false);

    const { project } = useContext(OrgContext);

    return (
        <Stack>
            <ContentProgress progressHidden={project !== undefined} />
            <ContentHeader title={project?.displayName}>
                <IconButton
                    toggle
                    checked={favorite}
                    onClick={() => setFavorate(!favorite)}
                    iconProps={{ iconName: favorite ? 'FavoriteStarFill' : 'FavoriteStar', color: 'yellow' }} />
            </ContentHeader>
            <ContentContainer>
                <Route exact path='/orgs/:orgId/projects/:projectId/settings'>
                    <ProjectSettingsOverview />
                </Route>
            </ContentContainer>
        </Stack>
    );
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Route } from 'react-router-dom';
import { Stack, IconButton } from '@fluentui/react';
import { Project, User } from 'teamcloud';
import { MembersForm, MemberForm, ProjectSettingsOverview, ContentHeader, ContentContainer, ContentProgress } from '../components';
import { ProjectMember } from '../model';

export interface IProjectSettingsViewProps {
    user?: User;
    project?: Project;
}

export const ProjectSettingsView: React.FC<IProjectSettingsViewProps> = (props) => {

    const [favorite, setFavorate] = useState(false);
    const [selectedMember, setSelectedMember] = useState<ProjectMember>();
    const [newUsersPanelOpen, setNewUsersPanelOpen] = useState(false);
    const [editUsersPanelOpen, setEditUsersPanelOpen] = useState(false);

    return (
        <>
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
            <MembersForm
                members={[]}
                panelIsOpen={newUsersPanelOpen}
                onFormClose={() => setNewUsersPanelOpen(false)} />
            <MemberForm
                user={selectedMember?.user}
                project={props.project}
                graphUser={selectedMember?.graphUser}
                panelIsOpen={editUsersPanelOpen}
                onFormClose={() => { setEditUsersPanelOpen(false); setSelectedMember(undefined) }} />
        </>
    );
}

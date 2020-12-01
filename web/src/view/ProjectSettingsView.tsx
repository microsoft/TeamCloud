// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Route, useParams } from 'react-router-dom';
import { Stack, IconButton } from '@fluentui/react';
import { useIsAuthenticated } from '@azure/msal-react';
import { Project, User } from 'teamcloud';
import { MembersForm, MemberForm, ProjectSettingsOverview, ContentHeader, ContentContainer, ContentProgress } from '../components';
import { ProjectMember } from '../model';
import { api } from '../API';

export interface IProjectSettingsViewProps {
    user?: User;
    project?: Project;
}

export const ProjectSettingsView: React.FC<IProjectSettingsViewProps> = (props) => {

    let isAuthenticated = useIsAuthenticated();
    let { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const [user, setUser] = useState(props.user);
    const [project, setProject] = useState(props.project);
    const [favorite, setFavorate] = useState(false);
    const [selectedMember, setSelectedMember] = useState<ProjectMember>();
    const [newUsersPanelOpen, setNewUsersPanelOpen] = useState(false);
    const [editUsersPanelOpen, setEditUsersPanelOpen] = useState(false);


    useEffect(() => {
        if (isAuthenticated && orgId && projectId) {

            if (project && (project.id === projectId || project.slug === projectId))
                return;

            setProject(undefined);

            const _setProject = async () => {
                const result = await api.getProject(projectId, orgId);
                setProject(result.data);
            };

            _setProject();
        }
    }, [isAuthenticated, project, projectId, orgId]);

    useEffect(() => {
        if (isAuthenticated && project && user === undefined) {

            setUser(undefined);

            const _setUser = async () => {
                const result = await api.getProjectUserMe(project.organization, project.id);
                setUser(result.data);
            };

            _setUser();
        }
    }, [isAuthenticated, project, user]);

    return (
        <>
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
                        <ProjectSettingsOverview {...{ project: project, user: user }} />
                    </Route>
                </ContentContainer>
            </Stack>
            <MembersForm
                members={[]}
                panelIsOpen={newUsersPanelOpen}
                onFormClose={() => setNewUsersPanelOpen(false)} />
            <MemberForm
                user={selectedMember?.user}
                project={project}
                graphUser={selectedMember?.graphUser}
                panelIsOpen={editUsersPanelOpen}
                onFormClose={() => { setEditUsersPanelOpen(false); setSelectedMember(undefined) }} />
        </>
    );
}

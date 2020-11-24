// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Route, useParams } from 'react-router-dom';
import { Stack, getTheme, IconButton } from '@fluentui/react';
import { useIsAuthenticated } from '@azure/msal-react';
import { Project, User } from 'teamcloud';
import { ProjectMembersForm, ProjectMemberForm, ProjectMembers, ProjectComponents, ProjectOverview, ContentHeader, ContentProgress, ContentContainer } from '../components';
import { ProjectMember } from '../model';
import { api } from '../API';

export interface IProjectViewProps {
    user?: User;
    project?: Project;
}

export const ProjectView: React.FunctionComponent<IProjectViewProps> = (props) => {

    let isAuthenticated = useIsAuthenticated();

    let { orgId, projectId, navId } = useParams() as { orgId: string, projectId: string, navId: string };

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

    const _onEditMember = (member?: ProjectMember) => {
        setSelectedMember(member)
        setEditUsersPanelOpen(true)
    };

    const theme = getTheme();

    return project?.id ? (
        <>
            <Stack>
                <ContentHeader title={navId ? navId : project.displayName} coin={!navId}>
                    {!navId && (
                        <IconButton
                            toggle
                            checked={favorite}
                            onClick={() => setFavorate(!favorite)}
                            iconProps={{ iconName: favorite ? 'FavoriteStarFill' : 'FavoriteStar', color: 'yellow' }} />
                    )}
                </ContentHeader>
                <ContentContainer>
                    <Route exact path='/orgs/:orgId/projects/:projectId'>
                        <ProjectOverview {...{ project: project, user: user }} />
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/componenets'>
                        <ProjectComponents {...{ project: project, user: user }} />
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/members'>
                        <ProjectMembers {...{ project: project, user: user, onEditMember: _onEditMember }} />
                    </Route>
                </ContentContainer>
            </Stack>
            <ProjectMembersForm
                project={project}
                panelIsOpen={newUsersPanelOpen}
                onFormClose={() => setNewUsersPanelOpen(false)} />
            <ProjectMemberForm
                user={selectedMember?.user}
                project={project}
                graphUser={selectedMember?.graphUser}
                panelIsOpen={editUsersPanelOpen}
                onFormClose={() => { setEditUsersPanelOpen(false); setSelectedMember(undefined) }} />
        </>
    ) : (<ContentProgress progressHidden={project !== undefined} />);
}

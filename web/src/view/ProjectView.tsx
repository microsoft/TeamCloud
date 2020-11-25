// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Route, useParams } from 'react-router-dom';
import { Stack, getTheme, IconButton } from '@fluentui/react';
import { useIsAuthenticated } from '@azure/msal-react';
import { Project, User, Component } from 'teamcloud';
import { ProjectMembersForm, ProjectMemberForm, ProjectMembers, ProjectComponents, ProjectOverview, ContentHeader, ContentProgress, ContentContainer } from '../components';
import { ProjectMember } from '../model';
import { api } from '../API';
import { getGraphDirectoryObject, getGraphUser } from '../MSGraph';

export interface IProjectViewProps {
    user?: User;
    project?: Project;
}

export const ProjectView: React.FunctionComponent<IProjectViewProps> = (props) => {

    let isAuthenticated = useIsAuthenticated();

    let { orgId, projectId, navId } = useParams() as { orgId: string, projectId: string, navId: string };

    const [user, setUser] = useState(props.user);
    const [project, setProject] = useState(props.project);
    const [members, setMembers] = useState<ProjectMember[]>();
    const [components, setComponents] = useState<Component[]>();
    const [favorite, setFavorate] = useState(false);
    const [selectedMember, setSelectedMember] = useState<ProjectMember>();
    const [newUsersPanelOpen, setNewUsersPanelOpen] = useState(false);
    const [editUsersPanelOpen, setEditUsersPanelOpen] = useState(false);


    useEffect(() => {
        if (isAuthenticated && orgId && projectId) {

            if (project && (project.id === projectId || project.slug === projectId))
                return;

            console.log('setProject');
            setProject(undefined);

            const _setProject = async () => {
                const result = await api.getProject(projectId, orgId);
                setProject(result.data);
            };

            _setProject();
        }
    }, [isAuthenticated, project, projectId, orgId]);


    useEffect(() => {
        if (project && (navId === undefined || navId === 'members')) {
            if (members && members.length > 0 && members[0].projectMembership.projectId === project.id)
                return;
            console.log('setProjectMembers');
            const _setMembers = async () => {
                let _users = await api.getProjectUsers(project.organization, project.id);
                if (_users.data) {
                    let _members = await Promise.all(_users.data.map(async u => ({
                        user: u,
                        graphUser: u.userType === 'User' ? await getGraphUser(u.id) : u.userType === 'Provider' ? await getGraphDirectoryObject(u.id) : undefined,
                        projectMembership: u.projectMemberships!.find(m => m.projectId === project.id)!
                    })));
                    setMembers(_members);
                }
            };
            _setMembers();
        }
    }, [isAuthenticated, project, navId]);


    useEffect(() => {
        if (project && (navId === undefined || navId === 'components')) {
            if (components === undefined || (components.length > 0 && components[0].projectId !== project.id)) {
                console.log('setProjectComponents');
                const _setComponents = async () => {
                    const result = await api.getProjectComponents(project.organization, project.id);
                    setComponents(result.data ?? undefined);
                };
                _setComponents();
            }
        }
    }, [isAuthenticated, project, navId]);


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
                        <ProjectOverview {...{ project: project, user: user, members: members, components: components }} />
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/componenets'>
                        <ProjectComponents {...{ project: project, user: user, components: components }} />
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/members'>
                        <ProjectMembers {...{ project: project, members: members, onEditMember: _onEditMember }} />
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

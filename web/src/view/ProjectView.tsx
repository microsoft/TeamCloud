// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Route, useParams } from 'react-router-dom';
import { Stack, IconButton } from '@fluentui/react';
import { useIsAuthenticated } from '@azure/msal-react';
import { Project, User, Component } from 'teamcloud';
import { MembersForm, MemberForm, ProjectOverview, ContentHeader, ContentProgress, ContentContainer, MemberList, ComponentList } from '../components';
import { ProjectMember } from '../model';
import { api } from '../API';
import { getGraphDirectoryObject, getGraphUser } from '../MSGraph';

export interface IProjectViewProps {
    user?: User;
    project?: Project;
}

export const ProjectView: React.FC<IProjectViewProps> = (props) => {

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
        if (isAuthenticated && project && user === undefined) {

            console.log('setUser');
            setUser(undefined);

            const _setUser = async () => {
                const result = await api.getProjectUserMe(project.organization, project.id);
                setUser(result.data);
            };

            _setUser();
        }
    }, [isAuthenticated, project, user]);

    useEffect(() => {
        if (isAuthenticated && project && (navId === undefined || navId === 'members')) {
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
    }, [isAuthenticated, project, members, navId]);


    useEffect(() => {
        if (isAuthenticated && project && (navId === undefined || navId === 'components')) {
            if (components === undefined || (components.length > 0 && components[0].projectId !== project.id)) {
                console.log('setProjectComponents');
                const _setComponents = async () => {
                    const result = await api.getProjectComponents(project.organization, project.id);
                    setComponents(result.data ?? undefined);
                };
                _setComponents();
            }
        }
    }, [isAuthenticated, project, components, navId]);

    return (
        <>
            <Stack>
                <ContentProgress progressHidden={project !== undefined} />
                <ContentHeader title={navId ? navId : project?.displayName} coin={!navId}>
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
                    <Route exact path='/orgs/:orgId/projects/:projectId/components'>
                        <ComponentList {...{ project: project, user: user, components: components }} />
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/members'>
                        <MemberList {...{ project: project, members: members }} />
                    </Route>
                </ContentContainer>
            </Stack>
            <MembersForm
                members={members}
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

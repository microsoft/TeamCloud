// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect, useContext } from 'react';
import { Route, Switch, useHistory, useLocation, useParams } from 'react-router-dom';
import { Stack, IconButton } from '@fluentui/react';
import { useIsAuthenticated } from '@azure/msal-react';
import { Component, ComponentTemplate, UserDefinition } from 'teamcloud';
import { ProjectOverview, ContentHeader, ContentProgress, ContentContainer, MemberList, ComponentList, ComponentForm } from '../components';
import { ProjectMember } from '../model';
import { api } from '../API';
import { getGraphUser } from '../MSGraph';
import { OrgContext, ProjectContext } from '../Context';

export const ProjectView: React.FC = () => {

    const location = useLocation();
    const history = useHistory();
    const { navId } = useParams() as { orgId: string, navId: string };

    const [favorite, setFavorate] = useState(false);

    const isAuthenticated = useIsAuthenticated();
    const [members, setMembers] = useState<ProjectMember[]>();
    const [components, setComponents] = useState<Component[]>();
    const [templates, setTemplates] = useState<ComponentTemplate[]>();

    const { org, project, user } = useContext(OrgContext);

    useEffect(() => {
        if (isAuthenticated && project && (navId === undefined || navId.toLowerCase() === 'members')) {
            if (members && members.length > 0 && members[0].projectMembership.projectId === project.id)
                return;
            const _setMembers = async () => {
                console.log(`setProjectMembers (${project.slug})`);
                let _users = await api.getProjectUsers(project!.organization, project!.id);
                if (_users.data) {
                    let _members = await Promise.all(_users.data.map(async u => ({
                        user: u,
                        graphUser: await getGraphUser(u.id),
                        projectMembership: u.projectMemberships!.find(m => m.projectId === project!.id)!
                    })));
                    setMembers(_members);
                }
            };
            _setMembers();
        }
    }, [isAuthenticated, project, members, navId]);


    useEffect(() => {
        if (isAuthenticated && project && (navId === undefined || (navId.toLowerCase() === 'components' && !location.pathname.toLowerCase().endsWith('/new')))) {
            if (components === undefined || (components.length > 0 && components[0].projectId !== project.id)) {
                const _setComponents = async () => {
                    console.log(`setProjectComponents (${project.slug})`);
                    const result = await api.getProjectComponents(project!.organization, project!.id);
                    setComponents(result.data ?? undefined);
                };
                _setComponents();
            }
        }
    }, [isAuthenticated, project, components, navId, location]);


    useEffect(() => {
        if (isAuthenticated && project && navId?.toLowerCase() === 'components' && location.pathname.toLowerCase().endsWith('/new')) {
            if (templates === undefined) {
                const _setTemplates = async () => {
                    console.log(`setProjectComponentTemplates (${project.slug})`);
                    const result = await api.getProjectComponentTemplates(project!.organization, project!.id);
                    setTemplates(result.data ?? undefined);
                };
                _setTemplates();
            }
        }
    }, [isAuthenticated, project, templates, navId, location]);


    const onAddUsers = async (users: UserDefinition[]) => {
        if (project) {
            console.log(`addMembers (${project.slug})`);
            const results = await Promise
                .all(users.map(async d => await api.createProjectUser(project.organization, project.id, { body: d })));

            results.forEach(r => {
                if (!r.data)
                    console.error(r);
            });

            const newMembers = await Promise.all(results
                .filter(r => r.data)
                .map(r => r.data!)
                .map(async u => ({
                    user: u,
                    graphUser: await getGraphUser(u.id),
                    projectMembership: u.projectMemberships!.find(m => m.projectId === project.id)!
                })));

            setMembers(members ? [...members, ...newMembers] : newMembers)
        }
    }

    // const { members, components, onAddUsers } = useProject(isAuthenticated, project, navId);
    // const { navId, user, project, members, components, onAddUsers } = useProject();

    return (
        <ProjectContext.Provider value={{
            org: org,
            user: user,
            project: project,
            members: members,
            components: components,
            templates: templates,
            onAddUsers: onAddUsers
        }}>
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
                        <ContentProgress progressHidden={project !== undefined && templates !== undefined} />
                        <ContentHeader title='New Component'>
                            <IconButton iconProps={{ iconName: 'ChromeClose' }}
                                onClick={() => history.push(`/orgs/${org?.slug}/projects/${project?.slug}`)} />
                        </ContentHeader>
                        <ContentContainer>
                            <ComponentForm />
                        </ContentContainer>
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/components'>
                        <ContentProgress progressHidden={project !== undefined && components !== undefined} />
                        <ContentHeader title={navId} />
                        <ContentContainer>
                            <ComponentList />
                        </ContentContainer>
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/members'>
                        <ContentProgress progressHidden={project !== undefined && members !== undefined} />
                        <ContentHeader title={navId} />
                        <ContentContainer>
                            <MemberList {...{ project: project, members: members, onAddUsers: onAddUsers }} />
                        </ContentContainer>
                    </Route>
                </Switch>
            </Stack>
        </ProjectContext.Provider>
    );
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect, useContext, useCallback } from 'react';
import { Route, Switch, useHistory, useLocation, useParams } from 'react-router-dom';
import { Stack, IconButton } from '@fluentui/react';
import { useIsAuthenticated } from '@azure/msal-react';
import { Component, ComponentTemplate, ProjectComponentDefinition, UserDefinition } from 'teamcloud';
import { ProjectOverview, ContentHeader, ContentProgress, ContentContainer, MemberList, ComponentList, ComponentForm, ProjectSettingsOverview } from '../components';
import { ProjectMember } from '../model';
import { api } from '../API';
import { getGraphUser } from '../MSGraph';
import { OrgContext, ProjectContext } from '../Context';
import { ComponentOverview } from '../components/ComponentOverview';

export const ProjectView: React.FC = () => {

    const isAuthenticated = useIsAuthenticated();

    const location = useLocation();
    const history = useHistory();
    const { navId, itemId } = useParams() as { orgId: string, navId: string, itemId: string };

    const [favorite, setFavorate] = useState(false);

    const [members, setMembers] = useState<ProjectMember[]>();
    const [components, setComponents] = useState<Component[]>();
    const [templates, setTemplates] = useState<ComponentTemplate[]>();
    const [progressHidden, setProgressHidden] = useState(true);

    const [selectedComponent, setSelectedComponent] = useState<Component>();

    const { org, project, user } = useContext(OrgContext);

    useEffect(() => { // Members
        if (isAuthenticated && project) {
            if (navId === undefined || navId.toLowerCase() === 'members' || navId.toLowerCase() === 'components') {
                if (members === undefined || (members.length > 0 && members[0].projectMembership.projectId !== project.id)) {
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
            }
        } else if (members) {
            console.log('setProjectMembers (undefined)');
            setMembers(undefined);
        }
    }, [isAuthenticated, project, members, navId]);


    useEffect(() => { // Components
        if (isAuthenticated && project) {
            if ((navId === undefined && !location.pathname.toLowerCase().endsWith('/settings'))
                || (navId && navId.toLowerCase() === 'components' && !location.pathname.toLowerCase().endsWith('/new'))) {
                if (components === undefined || (components.length > 0 && components[0].projectId !== project.id)) {
                    const _setComponents = async () => {
                        console.log(`setProjectComponents (${project.slug})`);
                        const result = await api.getProjectComponents(project!.organization, project!.id);
                        setComponents(result.data ?? undefined);
                    };
                    _setComponents();
                }
            }
        } else if (components) {
            console.log('setProjectComponents (undefined)');
            setComponents(undefined);
        }
    }, [isAuthenticated, project, components, navId, location]);


    useEffect(() => {// Component Templates
        if (isAuthenticated && project) {
            // if (navId?.toLowerCase() === 'components') {
            if (navId === undefined || navId.toLowerCase() === 'components') {
                // if (templates === undefined || (templates.length > 0 && templates[0].projectId !== project.id)) {
                if (templates === undefined) {
                    const _setTemplates = async () => {
                        console.log(`setProjectComponentTemplates (${project.slug})`);
                        const result = await api.getProjectComponentTemplates(project!.organization, project!.id);
                        setTemplates(result.data ?? undefined);
                    };
                    _setTemplates();
                }
            }
        } else if (templates) {
            console.log('setProjectComponentTemplates (undefined)');
            setTemplates(undefined);
        }
    }, [isAuthenticated, project, templates, navId]);


    const onComponentSelected = useCallback((component?: Component) => {
        if (component && selectedComponent && selectedComponent.id === component.id)
            return;
        console.log(`setComponent (${project?.slug})`);
        setSelectedComponent(component);
    }, [project, selectedComponent]);


    useEffect(() => { // Esure selected item matches route
        if (itemId) {
            // if (selectedComponent && (selectedComponent.id.toLowerCase() === itemId.toLowerCase() || selectedComponent.slug.toLowerCase() === itemId.toLowerCase())) {
            if (selectedComponent && selectedComponent.id.toLowerCase() === itemId.toLowerCase()) {
                return;
            } else if (components) {
                // const find = components.find(c => c.id.toLowerCase() === itemId.toLowerCase() || c.slug.toLowerCase() === projectId.toLowerCase());
                const find = components.find(c => c.id.toLowerCase() === itemId.toLowerCase());
                if (find) {
                    console.log(`getComponentFromRoute (${itemId})`);
                    onComponentSelected(find);
                }
            }
        } else if (selectedComponent) {
            console.log(`getComponentFromRoute (undefined)`);
            onComponentSelected(undefined);
        }
    }, [itemId, components, selectedComponent, onComponentSelected]);


    const onAddUsers = async (users: UserDefinition[]) => {
        if (project) {
            setProgressHidden(false);
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

            setMembers(members ? [...members, ...newMembers] : newMembers);
            setProgressHidden(true);
        }
    };


    const onCreateComponent = async (component: ProjectComponentDefinition) => {
        if (project) {
            setProgressHidden(false);
            console.log(`createComponent (${project.slug})`);
            const result = await api.createProjectComponent(project.organization, project.id, { body: component });
            if (result.data) {
                setComponents(components ? [...components, result.data] : [result.data]);
            } else {
                console.error(result);
            }
        } else {
            console.error('No project specified');
        }
        setProgressHidden(true);
    };

    // const { members, components, onAddUsers } = useProject(isAuthenticated, project, navId);
    // const { navId, user, project, members, components, onAddUsers } = useProject();

    return (
        <ProjectContext.Provider value={{
            user: user,
            project: project,
            members: members,
            components: components,
            component: selectedComponent,
            templates: templates,
            onComponentSelected: onComponentSelected,
            onAddUsers: onAddUsers,
            onCreateComponent: onCreateComponent
        }}>
            <Stack>
                <Switch>
                    <Route exact path='/orgs/:orgId/projects/:projectId'>
                        <ContentProgress progressHidden={progressHidden && project !== undefined && components !== undefined && members !== undefined} />
                        <ContentHeader title={project?.displayName} coin>
                            <IconButton toggle checked={favorite} onClick={() => setFavorate(!favorite)}
                                iconProps={{ iconName: favorite ? 'FavoriteStarFill' : 'FavoriteStar', color: 'yellow' }} />
                        </ContentHeader>
                        <ContentContainer>
                            <ProjectOverview />
                        </ContentContainer>
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/components/new'>
                        <ContentProgress progressHidden={progressHidden && project !== undefined && templates !== undefined} />
                        <ContentHeader title='New Component'>
                            <IconButton iconProps={{ iconName: 'ChromeClose' }}
                                onClick={() => history.push(`/orgs/${org?.slug}/projects/${project?.slug}`)} />
                        </ContentHeader>
                        <ContentContainer>
                            <ComponentForm />
                        </ContentContainer>
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/components'>
                        <ContentProgress progressHidden={progressHidden && project !== undefined && components !== undefined && templates !== undefined && members !== undefined} />
                        <ContentHeader title={navId} />
                        <ContentContainer>
                            <ComponentList />
                        </ContentContainer>
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/components/:itemId'>
                        <ContentProgress progressHidden={progressHidden && project !== undefined && components !== undefined && templates !== undefined && members !== undefined} />
                        <ContentHeader title={selectedComponent?.displayName ?? undefined} />
                        <ContentContainer>
                            <ComponentOverview />
                        </ContentContainer>
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/members'>
                        <ContentProgress progressHidden={progressHidden && project !== undefined && members !== undefined} />
                        <ContentHeader title={navId} />
                        <ContentContainer>
                            <MemberList {...{ project: project, members: members, onAddUsers: onAddUsers }} />
                        </ContentContainer>
                    </Route>
                    <Route exact path='/orgs/:orgId/projects/:projectId/settings'>
                        <ContentProgress progressHidden={progressHidden && project !== undefined && members !== undefined} />
                        <ContentHeader title={`${(project?.displayName ? (project.displayName + ' - Settings') : 'Settings')}`} coin={project?.displayName !== undefined} />
                        <ContentContainer>
                            <ProjectSettingsOverview />
                        </ContentContainer>
                    </Route>
                </Switch>
            </Stack>
        </ProjectContext.Provider>
    );
}

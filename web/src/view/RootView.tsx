// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useCallback, useEffect, useState } from 'react';
import { InteractionType } from '@azure/msal-browser';
import { AuthenticatedTemplate, MsalAuthenticationResult, useIsAuthenticated, useMsalAuthentication } from '@azure/msal-react';
import { Redirect, Route, Switch, useLocation, useParams } from 'react-router-dom';
import { getTheme, Stack } from '@fluentui/react';
import { api, auth } from '../API';
import { GraphUser, ManagementGroup, Member, Subscription } from '../model';
import { getGraphUser, getMe } from '../MSGraph';
import { DeploymentScope, DeploymentScopeDefinition, Organization, Project, ProjectTemplate, ProjectTemplateDefinition, User, UserDefinition } from 'teamcloud';
import { ContentView, NavView } from '../view';
import { GraphUserContext, OrgContext } from '../Context'
import { getManagementGroups, getSubscriptions } from '../Azure';
import { HeaderBar } from '../components';


export interface IRootViewProps { }

export const RootView: React.FC<IRootViewProps> = (props) => {

    return (
        <Switch>
            <Redirect exact from='/orgs' to='/' />
            <Redirect exact from='/orgs/:orgId/projects' to='/orgs/:orgId' />
            <Redirect exact from='/orgs/:orgId/settings/overview' to='/orgs/:orgId/settings' />
            <Redirect exact from='/orgs/:orgId/projects/:projectId/overview' to='/orgs/:orgId/projects/:projectId' />
            <Redirect exact from='/orgs/:orgId/projects/:projectId/settings/overview' to='/orgs/:orgId/projects/:projectId/settings' />
            <Route exact path={[
                '/',
                '/orgs/new',
                '/orgs/:orgId',
                '/orgs/:orgId/settings',
                '/orgs/:orgId/settings/:settingId',
                '/orgs/:orgId/settings/:settingId/new',
                '/orgs/:orgId/projects/new',
                '/orgs/:orgId/projects/:projectId',
                '/orgs/:orgId/projects/:projectId/settings',
                '/orgs/:orgId/projects/:projectId/settings/:settingId',
                '/orgs/:orgId/projects/:projectId/:navId',
                '/orgs/:orgId/projects/:projectId/:navId/new',
                '/orgs/:orgId/projects/:projectId/:navId/:itemId',
            ]}>
                <StateRouter {...{}}>
                    {props.children}
                </StateRouter>
            </Route>
        </Switch>
    );
}

export interface IStateRouterProps { }

export const StateRouter: React.FC<IStateRouterProps> = (props) => {

    const { orgId, projectId, settingId } = useParams() as { orgId: string, projectId: string, navId: string, settingId: string };

    const isAuthenticated = useIsAuthenticated();

    const location = useLocation();

    const authResult: MsalAuthenticationResult = useMsalAuthentication(InteractionType.Redirect, { scopes: auth.getScopes() });

    useEffect(() => {
        if (authResult.error) {
            console.log('logging in...')
            authResult.login(InteractionType.Redirect, { scopes: auth.getScopes() });
        }
    }, [authResult]);


    const [orgs, setOrgs] = useState<Organization[]>();
    const [user, setUser] = useState<User>();
    const [projects, setProjects] = useState<Project[]>();
    const [members, setMembers] = useState<Member[]>();
    const [scopes, setScopes] = useState<DeploymentScope[]>();
    const [templates, setTemplates] = useState<ProjectTemplate[]>();
    const [selectedOrg, setSelectedOrg] = useState<Organization>();
    const [selectedProject, setSelectedProject] = useState<Project>();
    const [graphUser, setGraphUser] = useState<GraphUser>();

    const [subscriptions, setSubscriptions] = useState<Subscription[]>();
    const [managementGroups, setManagementGroups] = useState<ManagementGroup[]>();


    useEffect(() => { // Graph User
        if (isAuthenticated) {
            if (graphUser === undefined) {
                const _setGraphUser = async () => {
                    console.log(`setGraphUser`);
                    const result = await getMe();
                    setGraphUser(result);
                };
                _setGraphUser();
            }
        } else if (graphUser) {
            console.log(`setGraphUser (undefined)`);
            setGraphUser(undefined);
        }
    }, [isAuthenticated, graphUser]);


    useEffect(() => { // Orgs
        if (isAuthenticated) {
            if (orgs === undefined || (selectedOrg && !orgs.some(o => o.id === selectedOrg.id))) {
                const _setOrgs = async () => {
                    console.log('setOrgs');
                    const result = await api.getOrganizations();
                    setOrgs(result.data ?? undefined);
                };
                _setOrgs();
            }
        } else if (orgs) {
            console.log('setOrgs (undefined)');
            setOrgs(undefined);
        }
    }, [isAuthenticated, selectedOrg, orgs]);


    useEffect(() => { // User
        if (isAuthenticated && selectedOrg) {
            if (user === undefined || user.organization !== selectedOrg.id) {
                const _setUser = async () => {
                    console.log(`setUser (${selectedOrg.slug})`);
                    const result = await api.getOrganizationUserMe(selectedOrg.id);
                    setUser(result.data ?? undefined);
                };
                _setUser();
            }
        } else if (user) {
            console.log('setUser (undefined)');
            setUser(undefined);
        }
    }, [isAuthenticated, selectedOrg, user]);


    useEffect(() => { // Projects
        if (isAuthenticated && selectedOrg) {
            if (projects === undefined
                || (projects.length > 0 && projects[0].organization !== selectedOrg.id)
                || (selectedProject && !projects.some(p => p.id === selectedProject.id))) {
                const _setProjects = async () => {
                    console.log(`setProjects (${selectedOrg.slug})`);
                    const result = await api.getProjects(selectedOrg.id);
                    setProjects(result.data ?? undefined);
                };
                _setProjects();
            }
        } else if (projects) {
            console.log('setProjects (undefined)');
            setProjects(undefined);
        }
    }, [isAuthenticated, selectedOrg, projects, selectedProject]);


    useEffect(() => { // Members
        if (isAuthenticated && selectedOrg) {
            if (projectId === undefined && location.pathname.toLowerCase().includes('/settings')
                && (settingId === undefined || settingId.toLowerCase() === 'members')
                && (members === undefined || (members.length > 0 && members[0].user.organization !== selectedOrg.id))) {
                const _setMembers = async () => {
                    console.log(`setMembers (${selectedOrg.slug})`);
                    let _users = await api.getOrganizationUsers(selectedOrg.id);
                    if (_users.data) {
                        let _members = await Promise.all(_users.data.map(async u => ({
                            user: u,
                            graphUser: await getGraphUser(u.id)
                        })));
                        setMembers(_members);
                    }
                };
                _setMembers();
            }
        } else if (members) {
            console.log('setMembers (undefined)');
            setMembers(undefined);
        }
    }, [isAuthenticated, selectedOrg, members, projectId, settingId, location]);


    useEffect(() => { // Deployment Scopes
        if (isAuthenticated && selectedOrg) {
            if (scopes === undefined || (scopes.length > 0 && scopes[0].organization !== selectedOrg.id)) {
                const _setScopes = async () => {
                    console.log(`setDeploymentScopes (${selectedOrg.slug})`);
                    let _scopes = await api.getDeploymentScopes(selectedOrg.id);
                    setScopes(_scopes.data ?? undefined)
                };
                _setScopes();
            }
        } else if (scopes) {
            console.log('setDeploymentScopes (undefined)');
            setTemplates(undefined);
        }
    }, [isAuthenticated, selectedOrg, scopes]);


    useEffect(() => { // Project Templates
        if (isAuthenticated && selectedOrg) {
            if (((projectId === undefined && settingId === 'templates') || location.pathname.toLowerCase().endsWith('/projects/new'))
                && (templates === undefined || (templates.length > 0 && templates[0].organization !== selectedOrg.id))) {
                const _setTemplates = async () => {
                    console.log(`setProjectTemplates (${selectedOrg.slug})`);
                    let _templates = await api.getProjectTemplates(selectedOrg.id);
                    setTemplates(_templates.data ?? undefined)
                };
                _setTemplates();
            }
        } else if (templates) {
            console.log('setProjectTemplates (undefined)');
            setTemplates(undefined);
        }
    }, [isAuthenticated, selectedOrg, templates, projectId, settingId, location]);


    useEffect(() => { // Project
        if ((!selectedOrg && selectedProject) || (selectedOrg && selectedProject && selectedOrg.id !== selectedProject.organization)) {
            console.log('setProject (undefined)');
            setSelectedProject(undefined);
        }
    }, [selectedOrg, selectedProject]);


    const onOrgSelected = useCallback((org?: Organization) => {
        if (org && selectedOrg && selectedOrg.id === org.id)
            return;
        console.log(`setOrg (${org?.slug})`);
        setSelectedOrg(org);
        setProjects(undefined);
    }, [selectedOrg]);


    const onProjectSelected = useCallback((project?: Project) => {
        if (project && selectedProject && selectedProject.id === project.id)
            return;
        console.log(`setProject (${project?.slug})`);
        setSelectedProject(project);
    }, [selectedProject]);


    useEffect(() => { // Ensure selected Org matches route
        if (orgId) {
            if (selectedOrg && (selectedOrg.id.toLowerCase() === orgId.toLowerCase() || selectedOrg.slug.toLowerCase() === orgId.toLowerCase())) {
                return;
            } else if (orgs) {
                const find = orgs.find(o => o.id.toLowerCase() === orgId.toLowerCase() || o.slug.toLowerCase() === orgId.toLowerCase());
                if (find) {
                    console.log(`getOrgFromRoute (${orgId})`);
                    onOrgSelected(find);
                }
            }
        } else if (selectedOrg) {
            console.log(`getOrgFromRoute (undefined)`);
            onOrgSelected(undefined);
        }
    }, [orgId, selectedOrg, orgs, onOrgSelected]);


    useEffect(() => { // Esure selected Project matches route
        if (projectId) {
            if (selectedProject && (selectedProject.id.toLowerCase() === projectId.toLowerCase() || selectedProject.slug.toLowerCase() === projectId.toLowerCase())) {
                return;
            } else if (projects) {
                const find = projects.find(p => p.id.toLowerCase() === projectId.toLowerCase() || p.slug.toLowerCase() === projectId.toLowerCase());
                if (find) {
                    console.log(`getProjectFromRoute (${projectId})`);
                    onProjectSelected(find);
                }
            }
        } else if (selectedProject) {
            console.log(`getProjectFromRoute (undefined)`);
            onProjectSelected(undefined);
        }
    }, [projectId, selectedProject, projects, onProjectSelected]);


    const onCreateDeploymentScope = async (scope: DeploymentScopeDefinition, org?: Organization) => {
        if (org ?? selectedOrg) {
            const result = await api.createDeploymentScope(org?.id ?? selectedOrg!.id, { body: scope, });
            if (result.data) {
                console.log(`createTemplate (${org?.slug ?? selectedOrg!.slug})`);
                setScopes(scopes ? [...scopes, result.data] : [result.data]);
            } else {
                console.error(`Failed to create new DeploymentScope: ${result}`);
            }
        }
    };


    const onCreateProjectTemplate = async (template: ProjectTemplateDefinition, org?: Organization) => {
        if (org || selectedOrg) {
            const result = await api.createProjectTemplate(org?.id ?? selectedOrg!.id, { body: template, });
            if (result.data) {
                console.log(`createTemplate (${org?.slug ?? selectedOrg!.slug})`);
                if (selectedOrg)
                    setTemplates(templates ? [...templates, result.data] : [result.data]);
            } else {
                console.error(`Failed to create new ProjectTemplate: ${result}`);
            }
        }
    };


    const onAddUsers = async (users: UserDefinition[]) => {
        if (selectedOrg) {
            console.log(`addMembers (${selectedOrg.slug})`);
            const results = await Promise
                .all(users.map(async d => await api.createOrganizationUser(selectedOrg.id, { body: d })));

            results.forEach(r => {
                if (!r.data)
                    console.error(r);
            });

            const newMembers = await Promise.all(results
                .filter(r => r.data)
                .map(r => r.data!)
                .map(async u => ({
                    user: u,
                    graphUser: await getGraphUser(u.id)
                })));

            setMembers(members ? [...members, ...newMembers] : newMembers)
        }
    };


    useEffect(() => { // Azure Subscriptions
        if (isAuthenticated) {
            if ((location.pathname.toLowerCase().endsWith('/orgs/new') || location.pathname.toLowerCase().endsWith('/scopes/new'))
                && subscriptions === undefined) {
                const _setSubscriptions = async () => {
                    console.log(`setSubscriptions`);
                    try {
                        const subs = await getSubscriptions();
                        setSubscriptions(subs ?? []);
                    } catch (error) {
                        setSubscriptions([]);
                    }
                };
                _setSubscriptions();
            }
        } else if (subscriptions) {
            console.log(`setManagementGroups (undefined)`);
            setSubscriptions(undefined);
        }
    }, [isAuthenticated, subscriptions, location]);


    useEffect(() => {
        if (isAuthenticated) {
            if ((location.pathname.toLowerCase().endsWith('/orgs/new') || location.pathname.toLowerCase().endsWith('/scopes/new')) && managementGroups === undefined) {
                const _setManagementGroups = async () => {
                    console.log(`setManagementGroups`);
                    try {
                        const groups = await getManagementGroups();
                        setManagementGroups(groups ?? []);
                    } catch (error) {
                        setManagementGroups([]);
                    }
                };
                _setManagementGroups();
            }
        } else {
            console.log(`setManagementGroups (undefined)`);
            setManagementGroups(undefined);
        }
    }, [isAuthenticated, managementGroups, location]);


    const theme = getTheme();

    const leftStackStyles = {
        root: {
            width: '260px',
            paddingTop: '20px',
            paddingBottom: '10px',
            borderRight: `${theme.palette.neutralLight} solid 1px`
        }
    };

    const rightStackStyles = {
        root: {
            backgroundColor: theme.palette.neutralLighterAlt
        }
    };

    return (
        <GraphUserContext.Provider value={{
            graphUser: graphUser,
            setGraphUser: setGraphUser,
            subscriptions: subscriptions,
            managementGroups: managementGroups,
        }}>
            <OrgContext.Provider value={{
                orgs: orgs,
                org: selectedOrg,
                user: user,
                members: members,
                scopes: scopes,
                templates: templates,
                project: selectedProject,
                projects: projects,
                onAddUsers: onAddUsers,
                onOrgSelected: onOrgSelected,
                onProjectSelected: onProjectSelected,
                onCreateDeploymentScope: onCreateDeploymentScope,
                onCreateProjectTemplate: onCreateProjectTemplate
            }}>
                <Stack verticalFill>
                    <HeaderBar />
                    <AuthenticatedTemplate>
                        <Stack horizontal disableShrink verticalFill verticalAlign='stretch'>
                            <Stack.Item styles={leftStackStyles}>
                                <NavView />
                            </Stack.Item>
                            <Stack.Item grow styles={rightStackStyles}>
                                <ContentView />
                            </Stack.Item>
                        </Stack>
                    </AuthenticatedTemplate>
                </Stack>
            </OrgContext.Provider>
        </GraphUserContext.Provider>
    );
}

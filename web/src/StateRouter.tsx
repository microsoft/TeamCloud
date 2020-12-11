// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useCallback, useEffect, useState } from 'react';
import { InteractionType } from '@azure/msal-browser';
import { useLocation, useParams } from 'react-router-dom';
import { MsalAuthenticationResult, useIsAuthenticated, useMsalAuthentication } from '@azure/msal-react';
import { Component, ComponentDeployment, ComponentTemplate, DeploymentScope, DeploymentScopeDefinition, Organization, Project, ProjectTemplate, ProjectTemplateDefinition, User, UserDefinition } from 'teamcloud';
import { matchesRouteParam, matchesLowerCase, endsWithLowerCase, endsWithAnyLowerCase, includesLowerCase, matchesAnyLowerCase } from './Utils'
import { GraphUser, ManagementGroup, Member, ProjectMember, Subscription } from './model';
import { GraphUserContext, OrgContext, ProjectContext } from './Context'
import { getManagementGroups, getSubscriptions } from './Azure';
import { getGraphUser, getMe } from './MSGraph';
import { api, auth } from './API';

export interface IStateRouterProps { }

export const StateRouter: React.FC<IStateRouterProps> = (props) => {

    const { orgId, projectId, navId, itemId, settingId } = useParams() as { orgId: string, projectId: string, navId: string, itemId: string, settingId: string };

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
    const [graphUser, setGraphUser] = useState<GraphUser>();

    const [org, setOrg] = useState<Organization>();
    const [user, setUser] = useState<User>();
    const [scopes, setScopes] = useState<DeploymentScope[]>();
    const [members, setMembers] = useState<Member[]>();
    const [projects, setProjects] = useState<Project[]>();
    const [templates, setTemplates] = useState<ProjectTemplate[]>();

    const [project, setProject] = useState<Project>();
    const [projectMembers, setProjectMembers] = useState<ProjectMember[]>();
    const [projectComponent, setProjectComponent] = useState<Component>();
    const [projectComponents, setProjectComponents] = useState<Component[]>();
    const [projectComponentTemplates, setProjectComponentTemplates] = useState<ComponentTemplate[]>();

    const [projectComponentDeployments, setProjectComponentDeployments] = useState<ComponentDeployment[]>();

    const [subscriptions, setSubscriptions] = useState<Subscription[]>();
    const [managementGroups, setManagementGroups] = useState<ManagementGroup[]>();


    useEffect(() => { // Graph User
        if (isAuthenticated) {
            if (graphUser === undefined) {
                const _setGraphUser = async () => {
                    console.log(`- setGraphUser`);
                    const result = await getMe();
                    setGraphUser(result);
                    console.log(`+ setGraphUser`);
                };
                _setGraphUser();
            }
        } else if (graphUser) {
            console.log(`+ setGraphUser (undefined)`);
            setGraphUser(undefined);
        }
    }, [isAuthenticated, graphUser]);


    useEffect(() => { // Orgs
        if (isAuthenticated) {
            if (orgs === undefined || (org && !orgs.some(o => o.id === org.id))) {
                const _setOrgs = async () => {
                    console.log('- setOrgs');
                    const result = await api.getOrganizations();
                    setOrgs(result.data ?? undefined);
                    console.log('+ setOrgs');
                };
                _setOrgs();
            }
        } else if (orgs) {
            console.log('+ setOrgs (undefined)');
            setOrgs(undefined);
        }
    }, [isAuthenticated, org, orgs]);


    useEffect(() => { // User
        if (isAuthenticated && org) {
            if (user === undefined || user.organization !== org.id) {
                const _setUser = async () => {
                    console.log(`- setUser (${org.slug})`);
                    const result = await api.getOrganizationUserMe(org.id);
                    setUser(result.data ?? undefined);
                    console.log(`+ setUser (${org.slug})`);
                };
                _setUser();
            }
        } else if (user) {
            console.log('+ setUser (undefined)');
            setUser(undefined);
        }
    }, [isAuthenticated, org, user]);


    useEffect(() => { // Projects
        if (isAuthenticated && org) {
            if (projects === undefined || projects.some(p => p.organization !== org.id)
                || (project && !projects.some(p => p.id === project.id))) {
                const _setProjects = async () => {
                    console.log(`- setProjects (${org.slug})`);
                    const result = await api.getProjects(org.id);
                    setProjects(result.data ?? undefined);
                    console.log(`+ setProjects (${org.slug})`);
                };
                _setProjects();
            }
        } else if (projects) {
            console.log('+ setProjects (undefined)');
            setProjects(undefined);
        }
    }, [isAuthenticated, org, projects, project]);


    useEffect(() => { // Members
        if (isAuthenticated && org) {
            if (projectId === undefined && includesLowerCase(location.pathname, '/settings')
                && (settingId === undefined || matchesLowerCase(settingId, 'members'))
                && (members === undefined || members.some(m => m.user.organization !== org.id))) {
                const _setMembers = async () => {
                    console.log(`- setMembers (${org.slug})`);
                    let _users = await api.getOrganizationUsers(org.id);
                    if (_users.data) {
                        let _members = await Promise.all(_users.data.map(async u => ({
                            user: u,
                            graphUser: await getGraphUser(u.id)
                        })));
                        setMembers(_members);
                    }
                    console.log(`+ setMembers (${org.slug})`);
                };
                _setMembers();
            }
        } else if (members) {
            console.log('+ setMembers (undefined)');
            setMembers(undefined);
        }
    }, [isAuthenticated, org, members, projectId, settingId, location]);


    useEffect(() => { // Deployment Scopes
        if (isAuthenticated && org) {
            if (scopes === undefined || scopes.some(s => s.organization !== org.id)) {
                const _setScopes = async () => {
                    console.log(`- setDeploymentScopes (${org.slug})`);
                    let _scopes = await api.getDeploymentScopes(org.id);
                    setScopes(_scopes.data ?? undefined)
                    console.log(`+ setDeploymentScopes (${org.slug})`);
                };
                _setScopes();
            }
        } else if (scopes) {
            console.log('+ setDeploymentScopes (undefined)');
            setTemplates(undefined);
        }
    }, [isAuthenticated, org, scopes]);


    useEffect(() => { // Project Templates
        if (isAuthenticated && org) {
            if (((projectId === undefined && matchesLowerCase(settingId, 'templates')) || endsWithLowerCase(location.pathname, '/projects/new'))
                && (templates === undefined || templates.some(t => t.organization !== org.id))) {
                const _setTemplates = async () => {
                    console.log(`- setProjectTemplates (${org.slug})`);
                    let _templates = await api.getProjectTemplates(org.id);
                    setTemplates(_templates.data ?? undefined)
                    console.log(`+ setProjectTemplates (${org.slug})`);
                };
                _setTemplates();
            }
        } else if (templates) {
            console.log('+ setProjectTemplates (undefined)');
            setTemplates(undefined);
        }
    }, [isAuthenticated, org, templates, projectId, settingId, location]);


    useEffect(() => { // Project
        if ((!org && project) || (org && project && org.id !== project.organization)) {
            console.log('+ setProject (undefined)');
            setProject(undefined);
        }
    }, [org, project]);


    const onOrgSelected = useCallback((selectedOrg?: Organization) => {
        if (selectedOrg && org && selectedOrg.id === org.id)
            return;
        console.log(`+ setOrg (${selectedOrg?.slug})`);
        setOrg(selectedOrg);
        setProjects(undefined);
    }, [org]);


    const onProjectSelected = useCallback((selectedProject?: Project) => {
        if (selectedProject && project && selectedProject.id === project.id)
            return;
        console.log(`+ setProject (${selectedProject?.slug})`);
        setProject(selectedProject);
    }, [project]);


    useEffect(() => { // Ensure selected Org matches route
        if (orgId) {
            if (org && matchesRouteParam(org, orgId)) {
                return;
            } else if (orgs) {
                const find = orgs.find(o => matchesRouteParam(o, orgId));
                if (find) {
                    console.log(`+ getOrgFromRoute (${orgId})`);
                    onOrgSelected(find);
                }
            }
        } else if (org) {
            console.log(`+ getOrgFromRoute (undefined)`);
            onOrgSelected(undefined);
        }
    }, [orgId, org, orgs, onOrgSelected]);


    useEffect(() => { // Esure selected Project matches route
        if (projectId) {
            if (project && matchesRouteParam(project, projectId)) {
                return;
            } else if (projects) {
                const find = projects.find(p => matchesRouteParam(p, projectId));
                if (find) {
                    console.log(`+ getProjectFromRoute (${projectId})`);
                    onProjectSelected(find);
                }
            }
        } else if (project) {
            console.log(`+ getProjectFromRoute (undefined)`);
            onProjectSelected(undefined);
        }
    }, [projectId, project, projects, onProjectSelected]);


    const onCreateDeploymentScope = async (scope: DeploymentScopeDefinition, parentOrg?: Organization) => {
        if (parentOrg ?? org) {
            console.log(`- createDeploymentScope (${parentOrg?.slug ?? org!.slug})`);
            const result = await api.createDeploymentScope(parentOrg?.id ?? org!.id, { body: scope, });
            if (result.data) {
                if (org) {
                    setScopes(scopes ? [...scopes, result.data] : [result.data]);
                }
            } else {
                console.error(`Failed to create new DeploymentScope: ${result}`);
            }
            console.log(`+ createDeploymentScope (${parentOrg?.slug ?? org!.slug})`);
        }
    };


    const onCreateProjectTemplate = async (template: ProjectTemplateDefinition, parentOrg?: Organization) => {
        if (parentOrg || org) {
            console.log(`- createTemplate (${parentOrg?.slug ?? org!.slug})`);
            const result = await api.createProjectTemplate(parentOrg?.id ?? org!.id, { body: template, });
            if (result.data) {
                if (org) {
                    setTemplates(templates ? [...templates, result.data] : [result.data]);
                }
            } else {
                console.error(`Failed to create new ProjectTemplate: ${result}`);
            }
            console.log(`+ createTemplate (${parentOrg?.slug ?? org!.slug})`);
        }
    };


    const onAddOrgUsers = async (users: UserDefinition[]) => {
        if (org) {
            console.log(`- addMembers (${org.slug})`);
            const results = await Promise
                .all(users.map(async d => await api.createOrganizationUser(org.id, { body: d })));

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
            console.log(`+ addMembers (${org.slug})`);
        }
    };


    useEffect(() => { // Azure Subscriptions
        if (isAuthenticated) {
            if (endsWithAnyLowerCase(location.pathname, '/orgs/new', '/scopes/new') && subscriptions === undefined) {
                const _setSubscriptions = async () => {
                    console.log(`- setSubscriptions`);
                    try {
                        const subs = await getSubscriptions();
                        setSubscriptions(subs ?? []);
                    } catch (error) {
                        setSubscriptions([]);
                    } finally {
                        console.log(`+ setSubscriptions`);
                    }
                };
                _setSubscriptions();
            }
        } else if (subscriptions) {
            console.log(`+ setSubscriptions (undefined)`);
            setSubscriptions(undefined);
        }
    }, [isAuthenticated, subscriptions, location]);


    useEffect(() => { // Azure Management Groups
        if (isAuthenticated) {
            if (endsWithAnyLowerCase(location.pathname, '/orgs/new', '/scopes/new') && managementGroups === undefined) {
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



    useEffect(() => { // Project Members
        if (isAuthenticated && projectId && project) {
            if (navId === undefined || matchesAnyLowerCase(navId, 'members', 'components')) {
                if (projectMembers === undefined || projectMembers.some(m => m.projectMembership.projectId !== project.id)) {
                    const _setProjectMembers = async () => {
                        console.log(`- setProjectMembers (${project.slug})`);
                        let _users = await api.getProjectUsers(project!.organization, project!.id);
                        if (_users.data) {
                            let _members = await Promise.all(_users.data.map(async u => ({
                                user: u,
                                graphUser: await getGraphUser(u.id),
                                projectMembership: u.projectMemberships!.find(m => m.projectId === project!.id)!
                            })));
                            setProjectMembers(_members);
                        }
                        console.log(`+ setProjectMembers (${project.slug})`);
                    };
                    _setProjectMembers();
                }
            }
        } else if (projectMembers) {
            console.log('+ setProjectMembers (undefined)');
            setProjectMembers(undefined);
        }
    }, [isAuthenticated, projectId, project, projectMembers, navId]);


    useEffect(() => { // Project Components
        if (isAuthenticated && projectId && project) {
            if ((navId === undefined && !endsWithLowerCase(location.pathname, '/settings'))
                || (navId && matchesLowerCase(navId, 'components'))) {
                if (projectComponents === undefined
                    || projectComponents.some(c => c.projectId !== project.id)
                    || (projectComponent && !projectComponents.some(c => c.id === projectComponent.id))) {
                    const _setProjectComponents = async () => {
                        console.log(`- setProjectComponents (${project.slug})`);
                        const result = await api.getProjectComponents(project!.organization, project!.id);
                        setProjectComponents(result.data ?? undefined);
                        console.log(`+ setProjectComponents (${project.slug})`);
                    };
                    _setProjectComponents();
                }
            }
        } else if (projectComponents) {
            console.log('+ setProjectComponents (undefined)');
            setProjectComponents(undefined);
        }
    }, [isAuthenticated, projectId, project, projectComponents, projectComponent, navId, location]);


    useEffect(() => {// Project Component Templates
        if (isAuthenticated && projectId && project) {
            if (navId === undefined || matchesLowerCase(navId, 'components')) {
                if (projectComponentTemplates === undefined) {
                    const _setComponentTemplates = async () => {
                        console.log(`- setProjectComponentTemplates (${project.slug})`);
                        const result = await api.getProjectComponentTemplates(project!.organization, project!.id);
                        setProjectComponentTemplates(result.data ?? undefined);
                        console.log(`+ setProjectComponentTemplates (${project.slug})`);
                    };
                    _setComponentTemplates();
                }
            }
        } else if (projectComponentTemplates) {
            console.log('+ setProjectComponentTemplates (undefined)');
            setProjectComponentTemplates(undefined);
        }
    }, [isAuthenticated, projectId, project, projectComponentTemplates, navId]);


    const onComponentSelected = useCallback((selectedComponent?: Component) => {
        if (selectedComponent && projectComponent && selectedComponent.id === projectComponent.id)
            return;
        console.log(`+ setComponent (${selectedComponent?.slug})`);
        setProjectComponent(selectedComponent);
    }, [projectComponent]);


    useEffect(() => { // Esure selected Project Component matches route
        if (projectId && navId && matchesLowerCase(navId, 'components') && itemId) {
            if (projectComponent && matchesRouteParam(projectComponent, itemId)) {
                return;
            } else if (projectComponents) {
                const find = projectComponents.find(c => matchesRouteParam(c, itemId));
                if (find) {
                    console.log(`+ getComponentFromRoute (${itemId})`);
                    onComponentSelected(find);
                }
            }
        } else if (projectComponent) {
            console.log(`+ getComponentFromRoute (undefined)`);
            onComponentSelected(undefined);
        }
    }, [itemId, navId, projectId, projectComponents, projectComponent, location, onComponentSelected]);



    useEffect(() => {// Project Component Deployments
        if (isAuthenticated && projectId && project && projectComponent) {
            if (matchesLowerCase(navId, 'components') && itemId && projectComponent && matchesRouteParam(projectComponent, itemId)) {
                if (projectComponentDeployments === undefined || projectComponentDeployments.some(d => d.componentId !== projectComponent.id)) {
                    const _setComponentDeployments = async () => {
                        console.log(`- setProjectComponentDeployments (${projectComponent.slug})`);
                        const result = await api.getProjectDeployments(project.organization, project.id, projectComponent.id);
                        setProjectComponentDeployments(result.data ?? undefined);
                        console.log(`+ setProjectComponentDeployments (${projectComponent.slug})`);
                    };
                    _setComponentDeployments();
                }
            }
        } else if (projectComponentDeployments) {
            console.log('+ setProjectComponentDeployments (undefined)');
            setProjectComponentDeployments(undefined);
        }
    }, [isAuthenticated, projectId, project, projectComponent, navId, itemId, projectComponentDeployments]);



    const onAddProjectUsers = async (users: UserDefinition[]) => {
        if (project) {
            console.log(`- addProjectMembers (${project.slug})`);
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

            setProjectMembers(projectMembers ? [...projectMembers, ...newMembers] : newMembers);
            console.log(`+ addProjectMembers (${project.slug})`);
        }
    };

    return (
        <GraphUserContext.Provider value={{
            graphUser: graphUser,
            setGraphUser: setGraphUser,
            subscriptions: subscriptions,
            managementGroups: managementGroups
        }}>
            <OrgContext.Provider value={{
                org: org,
                orgs: orgs,
                user: user,
                scopes: scopes,
                members: members,
                projects: projects,
                templates: templates,
                onAddUsers: onAddOrgUsers,
                onRemoveUsers: () => Promise.resolve(),
                onOrgSelected: onOrgSelected,
                onProjectSelected: onProjectSelected,
                onCreateDeploymentScope: onCreateDeploymentScope,
                onCreateProjectTemplate: onCreateProjectTemplate
            }}>
                <ProjectContext.Provider value={{
                    user: user,
                    project: project,
                    members: projectMembers,
                    component: projectComponent,
                    components: projectComponents,
                    templates: projectComponentTemplates,
                    componentDeployments: projectComponentDeployments,
                    onAddUsers: onAddProjectUsers,
                    onRemoveUsers: () => Promise.resolve(),
                    onComponentSelected: onComponentSelected
                }}>
                    {props.children}
                </ProjectContext.Provider>
            </OrgContext.Provider>
        </GraphUserContext.Provider>
    );
}

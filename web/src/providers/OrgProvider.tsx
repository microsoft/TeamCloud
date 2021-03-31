// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useCallback, useEffect, useState } from 'react';
import { useLocation, useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { DeploymentScope, DeploymentScopeDefinition, Organization, Project, ProjectTemplate, ProjectTemplateDefinition, User, UserDefinition } from 'teamcloud';
import { Member } from '../model';
import { OrgContext } from '../Context';
import { getGraphUser } from '../MSGraph';
import { endsWithLowerCase, includesLowerCase, matchesLowerCase, matchesRouteParam } from '../Utils';
import { api } from '../API';
import { useOrgs } from '../Hooks';

export const OrgProvider = (props: any) => {

    const { projectId, settingId } = useParams() as { projectId: string, settingId: string };

    const isAuthenticated = useIsAuthenticated();

    const location = useLocation();

    const [user, setUser] = useState<User>();
    const [scopes, setScopes] = useState<DeploymentScope[]>();
    const [members, setMembers] = useState<Member[]>();
    const [project, setProject] = useState<Project>();
    const [projects, setProjects] = useState<Project[]>();
    const [templates, setTemplates] = useState<ProjectTemplate[]>();

    const { org } = useOrgs();

    const onProjectSelected = useCallback((selectedProject?: Project) => {
        if (selectedProject && project && selectedProject.id === project.id)
            return;
        console.log(`+ setProject (${selectedProject?.slug})`);
        setProject(selectedProject);
    }, [project]);


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


    useEffect(() => { // User
        if (isAuthenticated && org) {
            // no user OR user is from wrong org
            if (user === undefined || user.organization !== org.id) {
                if (user !== undefined && user.organization !== org.id) {
                    console.log('+ setProjects (undefined)');
                    setProjects(undefined); // HACK incase the projects were an empty array
                }
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
            // no projects OR projects are from wrong org
            if (projects === undefined || projects.some(p => p.organization !== org.id)
                // OR selected project isn't in the projects list (new project was created)
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
            // on an org (not project) settings page
            if (projectId === undefined && includesLowerCase(location.pathname, '/settings')
                // AND on an org (not project) settings overview OR members page
                && (settingId === undefined || matchesLowerCase(settingId, 'members'))
                // AND no members OR members are from wrong org
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
            // no scopes OR scopes are from wrong org
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
            // on the org (not project) settings templats page OR on the new project page
            if (((projectId === undefined && matchesLowerCase(settingId, 'templates')) || endsWithLowerCase(location.pathname, '/projects/new'))
                // AND no templates OR templates are from wrong org
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
        // unset selected project if selected org is undefined OR selected project is not in selected org
        if ((!org && project) || (org && project && org.id !== project.organization)) {
            console.log('+ setProject (undefined)');
            setProject(undefined);
        }
    }, [org, project]);


    return <OrgContext.Provider value={{
        org: org,
        user: user,
        scopes: scopes,
        members: members,
        project: project,
        projects: projects,
        templates: templates,
        onAddUsers: onAddOrgUsers,
        onRemoveUsers: () => Promise.resolve(),
        onProjectSelected: onProjectSelected,
        onCreateDeploymentScope: onCreateDeploymentScope,
        onCreateProjectTemplate: onCreateProjectTemplate
    }} {...props} />

}

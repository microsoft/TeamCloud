// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { BrowserRouter } from 'react-router-dom';
import { Stack } from '@fluentui/react';
import { InteractionType } from '@azure/msal-browser';
import { AuthenticatedTemplate, MsalAuthenticationResult, useMsalAuthentication, useIsAuthenticated } from '@azure/msal-react';
import { ContentView, NavView, HeaderView } from './view';
import { api, auth } from './API';
import { GraphUser } from './model';
import { getMe } from './MSGraph';
import { DeploymentScope, Organization, Project, User } from 'teamcloud';
import { BodyView } from './view';
import { StateRouter } from './StateRouter';
import { GraphUserContext, OrgContext } from './Context'

interface IAppProps { }

export const App: React.FC<IAppProps> = () => {

    const isAuthenticated = useIsAuthenticated();

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
    const [scopes, setScopes] = useState<DeploymentScope[]>();
    // const [templates, setTemplates] = useState<ProjectTemplate[]>();
    const [selectedOrg, setSelectedOrg] = useState<Organization>();
    const [selectedProject, setSelectedProject] = useState<Project>();
    const [graphUser, setGraphUser] = useState<GraphUser>();


    useEffect(() => {
        if (isAuthenticated && graphUser === undefined) {
            const _setGraphUser = async () => {
                console.log(`setGraphUser`);
                const result = await getMe();
                setGraphUser(result);
            };
            _setGraphUser();
        }
    }, [isAuthenticated, graphUser]);


    useEffect(() => {
        if (isAuthenticated && (orgs === undefined || (selectedOrg && !orgs.some(o => o.id === selectedOrg.id)))) {
            const _setOrgs = async () => {
                console.log('setOrgs');
                const result = await api.getOrganizations();
                setOrgs(result.data ?? undefined);
            };
            _setOrgs();
        }
    }, [isAuthenticated, selectedOrg, orgs]);


    useEffect(() => {
        if (isAuthenticated) {
            if (!selectedOrg) {
                if (projects) {
                    console.log('setProjects (undefined)');
                    setProjects(undefined);
                }
            } else if (projects === undefined
                || (projects.length > 0 && projects[0].organization !== selectedOrg.id)
                || (selectedProject && !projects.some(p => p.id === selectedProject.id))) {
                const _setProjects = async () => {
                    console.log(`setProjects (${selectedOrg.slug})`);
                    const result = await api.getProjects(selectedOrg.id);
                    setProjects(result.data ?? undefined);
                };
                _setProjects();
            }
        }
    }, [isAuthenticated, selectedOrg, projects, selectedProject]);


    useEffect(() => {
        if ((!selectedOrg && selectedProject) || (selectedOrg && selectedProject && selectedOrg.id !== selectedProject.organization)) {
            console.log('setProject (undefined)');
            setSelectedProject(undefined);
        }
    }, [selectedOrg, selectedProject]);


    useEffect(() => {
        if (isAuthenticated) {
            if (!selectedOrg) {
                if (user) {
                    console.log('setUser (undefined)');
                    setUser(undefined);
                }
            } else if (user === undefined || user.organization !== selectedOrg.id) {
                const _setUser = async () => {
                    console.log(`setUser (${selectedOrg.slug})`);
                    const result = await api.getOrganizationUserMe(selectedOrg.id);
                    setUser(result.data ?? undefined);
                };
                _setUser();
            }
        }
    }, [isAuthenticated, selectedOrg, user]);


    useEffect(() => {
        if (isAuthenticated && selectedOrg
            // && (location.pathname.toLowerCase().endsWith('/settings/scopes') || location.pathname.toLowerCase().endsWith('components/new'))
            && scopes === undefined) {
            const _setScopes = async () => {
                console.log(`setDeploymentScopes (${selectedOrg.slug})`);
                let _scopes = await api.getDeploymentScopes(selectedOrg.id);
                setScopes(_scopes.data ?? undefined)
            };
            _setScopes();
        }
    }, [isAuthenticated, selectedOrg, scopes]);


    // useEffect(() => {
    //     if (isAuthenticated && selectedOrg
    //         // && location.pathname.toLowerCase().endsWith('/settings/templates') && !location.pathname.toLowerCase().endsWith('/new')
    //         && templates === undefined) {
    //         const _setTemplates = async () => {
    //             console.log(`setProjectTemplates (${selectedOrg.slug})`);
    //             let _templates = await api.getProjectTemplates(selectedOrg.id);
    //             setTemplates(_templates.data ?? undefined)
    //         };
    //         _setTemplates();
    //     }
    // }, [isAuthenticated, selectedOrg, templates]);


    const onOrgSelected = (org?: Organization) => {
        if (org && selectedOrg && selectedOrg.id === org.id)
            return;
        console.log(`setSelectedOrg (${org?.slug})`);
        setSelectedOrg(org);
        setProjects(undefined);
    };


    const onProjectSelected = (project?: Project) => {
        if (project && selectedProject && selectedProject.id === project.id)
            return;
        console.log(`setSelectedProject (${project?.slug})`);
        setSelectedProject(project);
    };


    return (
        <GraphUserContext.Provider value={{ graphUser: graphUser, setGraphUser: setGraphUser }}>
            <OrgContext.Provider value={{
                orgs: orgs,
                org: selectedOrg,
                onOrgSelected: onOrgSelected,
                user: user,
                scopes: scopes,
                // templates: templates,
                projects: projects,
                project: selectedProject,
                onProjectSelected: onProjectSelected
            }}>
                <Stack verticalFill>
                    <BrowserRouter>
                        <StateRouter>
                            <HeaderView />
                            <AuthenticatedTemplate>
                                <BodyView nav={<NavView />} content={<ContentView />} />
                            </AuthenticatedTemplate>
                        </StateRouter>
                    </BrowserRouter>
                </Stack>
            </OrgContext.Provider>
        </GraphUserContext.Provider>
    );
}

export default App;

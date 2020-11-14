// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { initializeIcons } from '@uifabric/icons';
import { BrowserRouter, Switch, Route, useParams } from 'react-router-dom';
import { HeaderBar, RootNav } from './components';
import { Error404, ProjectDetailView, ProjectsView } from './view';
import { GraphUser } from './model';
import { Organization, Project, User } from 'teamcloud';
import { getMe } from './MSGraph';
import { api } from './API';
import { getTheme, Nav, Stack } from '@fluentui/react';

interface IAppProps {
    onSignOut: () => void;
}


export const App: React.FunctionComponent<IAppProps> = (props) => {
    initializeIcons();

    const [user, setUser] = useState<User>();
    const [org, setOrg] = useState<Organization>();
    const [orgId, setOrgId] = useState<string>();
    const [orgs, setOrgs] = useState<Organization[]>();
    const [project, setProject] = useState<Project>();
    const [graphUser, setGraphUser] = useState<GraphUser>();

    useEffect(() => {
        if (graphUser === undefined) {
            console.error('getMe');
            const _setGraphUser = async () => {
                const result = await getMe();
                setGraphUser(result);
            };
            _setGraphUser();
        }
    }, [graphUser]);

    useEffect(() => {
        console.error(orgs)
        if (graphUser && orgs === undefined) {
            console.error('getOrganizations');
            const _setOrgs = async () => {
                const result = await api.getOrganizations();
                setOrgs(result.data ?? undefined);
                console.error(result.data)
            };
            _setOrgs();
        }
    }, [graphUser]);

    useEffect(() => {
        if (graphUser && org && (user === undefined || user.organization !== org.id)) {
            console.error('getOrganizationUserMe');
            const _setUser = async () => {
                const result = await api.getOrganizationUserMe(org.id);
                setUser(result.data)
            };
            _setUser();
        }
    }, [org, user]);

    // useEffect(() => {
    //     if (graphUser && orgId) {
    //         console.error('getOrganization(orgId)');
    //         const _setOrg = async () => {
    //             const result = await api.getOrganization(orgId);
    //             setOrg(result.data)
    //         };
    //         _setOrg();
    //     }
    // }, [graphUser, orgId]);

    const _onOrgSelected = (org?: Organization) => {
        setOrg(org);
    }

    const _onProjectSelected = (project?: Project) => {
        setProject(project);
    }

    const theme = getTheme();

    return (
        <Stack verticalFill>
            <HeaderBar graphUser={graphUser} onSignOut={props.onSignOut} />
            <BrowserRouter>
                <Stack horizontal disableShrink verticalFill verticalAlign='stretch'>
                    <Stack.Item styles={{ root: { width: '260px', paddingTop: '20px', paddingBottom: '10px', borderRight: `${theme.palette.neutralLight} solid 1px` } }}>
                        <Route path='/' exact={true}>
                            <RootNav {...{ orgs: orgs, onOrgSelected: _onOrgSelected }} />
                        </Route>
                        <Route path='/orgs/:orgId' exact={true}>
                            <RootNav {...{ orgs: orgs, onOrgSelected: _onOrgSelected }} />
                        </Route>
                        <Route path='/orgs/:orgId/projects/:projectId'>
                            <RootNav {...{ orgs: orgs, onOrgSelected: _onOrgSelected }} />
                        </Route>
                    </Stack.Item>
                    <Stack.Item grow styles={{ root: { backgroundColor: theme.palette.neutralLighterAlt } }}>
                        <Switch>
                            <Route path='/' exact={true}>
                                {/* <HeaderBar user={user} graphUser={graphUser} onSignOut={props.onSignOut} /> */}
                                {/* <RootNav orgs={orgs} /> */}
                                <></>
                                {/* <ProjectsView user={user} onProjectSelected={_onProjectSelected} /> */}
                            </Route>
                            <Route path='/orgs/:orgId' exact={true}>
                                {/* <HeaderBar user={user} graphUser={graphUser} onSignOut={props.onSignOut} /> */}
                                <ProjectsView {...{ org: org, user: user, onProjectSelected: _onProjectSelected }} />
                            </Route>
                            <Route path='/orgs/:orgId/projects/:projectId'>
                                {/* <HeaderBar user={user} graphUser={graphUser} onSignOut={props.onSignOut} /> */}
                                <ProjectDetailView {...{ project: project, user: user }} />
                            </Route>
                            <Route path='*'>
                                {/* <HeaderBar user={user} graphUser={graphUser} onSignOut={props.onSignOut} /> */}
                                <Error404 />
                            </Route>
                        </Switch>
                    </Stack.Item>
                </Stack>
            </BrowserRouter>
        </Stack>
    );
}

// interface IProjectViewWrapperProps {
//     user?: User;
//     project?: Project;
// }

// interface IProjectsViewWrapperProps {
//     org?: Organization;
//     user?: User;
//     onProjectSelected?: (project: Project) => void;
// }

// function ProjectViewWrapper(props: IProjectViewWrapperProps) {

//     return <ProjectDetailView project={props.project} user={props.user} />;
// }

// function ProjectsViewWrapper(props: IProjectsViewWrapperProps) {
//     let { orgId } = useParams() as { orgId: string };
//     return <ProjectsView user={props.user} onProjectSelected={props.onProjectSelected} />;
// }

export default App;

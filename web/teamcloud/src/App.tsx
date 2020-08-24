// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { initializeIcons } from '@uifabric/icons';
import { BrowserRouter, Switch, Route, useParams } from "react-router-dom";
import { HeaderBar } from './components';
import { Error404, ProjectDetailView, ProjectsView, ProvidersView, ProjectTypesView } from './view';
import { Project, User, DataResult } from './model';
import { getUser } from './API';
import { GraphUser, getMe } from './MSGraph';

interface IAppProps {
    onSignOut: () => void;
}

export const App: React.FunctionComponent<IAppProps> = (props) => {
    initializeIcons();

    const [user, setUser] = useState<User>();
    const [project, setProject] = useState<Project>();
    const [graphUser, setGraphUser] = useState<GraphUser>();

    useEffect(() => {
        if (graphUser === undefined) {
            const _setGraphUser = async () => {
                const result = await getMe();
                setGraphUser(result);
                if (result && user === undefined) {
                    const _setUser = async (gu: GraphUser) => {
                        const result = await getUser(gu.id);
                        const data = (result as DataResult<User>).data;
                        setUser(data)
                    };
                    _setUser(result);
                }
            };
            _setGraphUser();
        }
    }, [graphUser, user]);

    const _onProjectSelected = (project?: Project) => {
        setProject(project);
    }

    return (
        <BrowserRouter>
            <Switch>
                <Route path="/projects/:projectId">
                    <HeaderBar graphUser={graphUser} onSignOut={props.onSignOut} />
                    <ProjectViewWrapper {...{ project: project, user: user }} />
                </Route>
                <Route path="/" exact={true}>
                    <HeaderBar graphUser={graphUser} onSignOut={props.onSignOut} />
                    <ProjectsView user={user} onProjectSelected={_onProjectSelected} />
                </Route>
                <Route path="/projectTypes" exact={true}>
                    <HeaderBar graphUser={graphUser} onSignOut={props.onSignOut} />
                    <ProjectTypesView user={user} onProjectSelected={_onProjectSelected} />
                </Route>
                <Route path="/providers" exact={true}>
                    <HeaderBar graphUser={graphUser} onSignOut={props.onSignOut} />
                    <ProvidersView user={user} onProjectSelected={_onProjectSelected} />
                </Route>
                <Route path="*">
                    <HeaderBar graphUser={graphUser} onSignOut={props.onSignOut} />
                    <Error404 />;
                    </Route>
            </Switch>
        </BrowserRouter>
    );
}

interface IProjectViewWrapperProps {
    user?: User;
    project?: Project;
}

function ProjectViewWrapper(props: IProjectViewWrapperProps) {
    let { projectId } = useParams();
    return <ProjectDetailView projectId={projectId} project={props.project} user={props.user} />;
}

export default App;

import React, { useState } from 'react';
import { initializeIcons } from '@uifabric/icons';
import { BrowserRouter, Switch, Route, useParams } from "react-router-dom";
import { HeaderBar } from './components';
import { Error404, HomeView, ProjectView } from './view';
import { } from './view/ProjectView'
import { Project } from './model';

interface IAppProps {
    onSignOut: () => void;
    // onProjectSelected?: (project: Project) => void;
}

export const App: React.FunctionComponent<IAppProps> = (props) => {
    initializeIcons();

    const [project, setProject] = useState<Project>();

    const _onProjectSelected = (project: Project | undefined) => {
        if (project)
            console.log(project.id);
        setProject(project);
    }

    return (
        <BrowserRouter>
            <Switch>
                <Route path="/projects/:projectId">
                    <HeaderBar onSignOut={props.onSignOut} project={project} />
                    <ProjectViewWrapper {...{ project: project, onProjectSelected: _onProjectSelected }} />
                </Route>
                <Route path="/" exact={true}>
                    <HeaderBar onSignOut={props.onSignOut} project={project} />
                    <HomeView onProjectSelected={_onProjectSelected} />
                </Route>
                <Route path="*">
                    <HeaderBar onSignOut={props.onSignOut} project={project} />
                    <Error404 />;
                    </Route>
            </Switch>
        </BrowserRouter>
    );
}

interface IProjectViewWrapperProps {
    project?: Project;
    onProjectSelected?: (project: Project) => void;
}

function ProjectViewWrapper(props: IProjectViewWrapperProps) {
    let { projectId } = useParams();
    return <ProjectView projectId={projectId} project={props.project} onProjectSelected={props.onProjectSelected} />;
}

export default App;

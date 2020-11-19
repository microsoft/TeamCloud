// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Route, Switch } from 'react-router-dom';
import { Project } from 'teamcloud';
import { Error404, NewOrganizationView, NewProjectView, ProjectView, ProjectsView } from '.';


export interface IContentViewProps { }

export const ContentView: React.FunctionComponent<IContentViewProps> = (props: IContentViewProps) => {

    const [project, setProject] = useState<Project>();

    const onProjectSelected = (project: Project) => setProject(project);

    return (
        <Switch>
            <Route exact path='/'>
                <></>
            </Route>
            <Route exact path='/orgs/new'>
                <NewOrganizationView />
            </Route>
            <Route exact path='/orgs/:orgId'>
                <ProjectsView {...{ onProjectSelected: onProjectSelected }} />
            </Route>
            <Route exact path='/orgs/:orgId/projects/new'>
                <NewProjectView {...{}} />
            </Route>
            <Route exact path='/orgs/:orgId/settings'>
                <></>
            </Route>
            <Route exact path='/orgs/:orgId/settings/members'>
                <></>
            </Route>
            <Route exact path='/orgs/:orgId/settings/configuration'>
                <></>
            </Route>
            <Route exact path='/orgs/:orgId/settings/organization'>
                <></>
            </Route>
            <Route exact path='/orgs/:orgId/settings/scopes'>
                <></>
            </Route>
            <Route exact path='/orgs/:orgId/settings/templates'>
                <></>
            </Route>
            <Route exact path='/orgs/:orgId/settings/providers'>
                <></>
            </Route>

            <Route exact path='/orgs/:orgId/projects/:projectId'>
                <ProjectView {...{ project: project }} />
            </Route>
            <Route exact path='/orgs/:orgId/projects/:projectId/settings'>
                <></>
            </Route>
            <Route exact path='/orgs/:orgId/projects/:projectId/settings/components'>
                <></>
            </Route>
            <Route exact path='/orgs/:orgId/projects/:projectId/settings/members'>
                <></>
            </Route>

            <Route exact path='/orgs/:orgId/projects/:projectId/:navId'>
                <ProjectView {...{ project: project }} />
            </Route>

            <Route path='*'>
                <Error404 />
            </Route>
        </Switch>
    );
}

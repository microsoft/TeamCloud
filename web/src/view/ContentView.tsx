// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Route, Switch } from 'react-router-dom';
import { Project } from 'teamcloud';
import { Error404, NewOrgView, NewProjectView, ProjectView, ProjectsView, OrgSettingsView, ProjectSettingsView } from '.';
import { NewComponentView } from './NewComponentView';


export interface IContentViewProps { }

export const ContentView: React.FC<IContentViewProps> = (props: IContentViewProps) => {

    const [project, setProject] = useState<Project>();

    const onProjectSelected = (project: Project) => setProject(project);

    return (
        <Switch>
            <Route exact path='/'>
                <></>
            </Route>
            <Route exact path='/orgs/new'>
                <NewOrgView />
            </Route>
            <Route exact path='/orgs/:orgId'>
                <ProjectsView {...{ onProjectSelected: onProjectSelected }} />
            </Route>
            <Route exact path='/orgs/:orgId/projects/new'>
                <NewProjectView {...{}} />
            </Route>
            <Route exact path={['/orgs/:orgId/settings', '/orgs/:orgId/settings/:settingId', '/orgs/:orgId/settings/:settingId/new']}>
                <OrgSettingsView {...{}} />
            </Route>
            <Route exact path={['/orgs/:orgId/projects/:projectId/settings', '/orgs/:orgId/projects/:projectId/settings/:settingId']}>
                <ProjectSettingsView {...{ project: project }} />
            </Route>
            <Route exact path={['/orgs/:orgId/projects/:projectId', '/orgs/:orgId/projects/:projectId/:navId']}>
                <ProjectView {...{ project: project }} />
            </Route>
            <Route exact path='/orgs/:orgId/projects/:projectId/components/new'>
                <NewComponentView {...{ project: project }} />
            </Route>
            <Route path='*'>
                <Error404 />
            </Route>
        </Switch>
    );
}

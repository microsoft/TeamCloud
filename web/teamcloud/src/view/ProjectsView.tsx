// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from "react";
import { ICommandBarItemProps, SearchBox, Stack, IBreadcrumbItem } from '@fluentui/react';
import { getProjects } from '../API'
import { Project, DataResult, User, TeamCloudUserRole } from '../model'
import { ProjectList, ProjectForm, SubheaderBar } from "../components";

export interface IProjectsViewProps {
    user?: User;
    onProjectSelected?: (project: Project) => void;
}

export const ProjectsView: React.FunctionComponent<IProjectsViewProps> = (props) => {

    const [projects, setProjects] = useState<Project[]>();
    const [projectFilter, setProjectFilter] = useState<string>();
    const [newProjectPanelOpen, setNewProjectPanelOpen] = useState(false);

    useEffect(() => {
        if (projects === undefined) {
            const _setProjects = async () => {
                const result = await getProjects();
                const data = (result as DataResult<Project[]>).data;
                setProjects(data);
            };
            _setProjects();
        }
    }, [projects]);

    const _refresh = async () => {
        let result = await getProjects();
        let data = (result as DataResult<Project[]>).data;
        setProjects(data);
    }

    const _userCanCreateProjects = () => props.user?.role === TeamCloudUserRole.Admin || props.user?.role === TeamCloudUserRole.Creator;

    const _commandBarItems = (): ICommandBarItemProps[] => [
        { key: 'refresh', text: 'Refresh', iconProps: { iconName: 'refresh' }, onClick: () => { _refresh() } },
        { key: 'newProject', text: 'New project', iconProps: { iconName: 'NewTeamProject' }, onClick: () => { setNewProjectPanelOpen(true) }, disabled: !_userCanCreateProjects() }
    ];

    const _centerCommandBarItems: ICommandBarItemProps[] = [
        { key: 'search', onRender: () => <SearchBox className="searchBox" iconProps={{ iconName: 'Filter' }} placeholder="Filter" onChange={(_, filter) => setProjectFilter(filter)} /> }
    ];

    const _breadcrumbs: IBreadcrumbItem[] = [
        { text: '', key: 'root', href: '/', isCurrentItem: true }
    ];

    return (
        <>
            <Stack>
                <SubheaderBar
                    breadcrumbs={_breadcrumbs}
                    commandBarItems={_commandBarItems()}
                    centerCommandBarItems={_centerCommandBarItems} />
                <ProjectList
                    projects={projects}
                    projectFilter={projectFilter}
                    onProjectSelected={props.onProjectSelected} />
            </Stack>
            <ProjectForm
                user={props.user}
                panelIsOpen={newProjectPanelOpen}
                onFormClose={() => setNewProjectPanelOpen(false)} />
        </>
    );
}

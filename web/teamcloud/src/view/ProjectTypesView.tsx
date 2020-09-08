// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { ICommandBarItemProps, SearchBox, Stack, IBreadcrumbItem } from '@fluentui/react';
import { getProjectTypes } from '../API'
import { Project, DataResult, ProjectType, User, TeamCloudUserRole } from '../model'
import { SubheaderBar, ProjectTypeForm, ProjectTypeList, ProjectTypePanel } from "../components";

export interface IProjectTypesViewProps {
    user?: User;
    onProjectSelected?: (project: Project) => void;
}

export const ProjectTypesView: React.FunctionComponent<IProjectTypesViewProps> = (props) => {

    const [projectTypes, setProjectTypes] = useState<ProjectType[]>();
    const [projectTypeFilter, setProjectTypeFilter] = useState<string>();
    const [newProjectTypePanelOpen, setNewProjectTypePanelOpen] = useState(false);

    const [selectedProjectType, setSelectedProjectType] = useState<ProjectType>();
    const [detailsPanelOpen, setDetailsPanelOpen] = useState(false);

    useEffect(() => {
        if (projectTypes === undefined) {
            const _setProjectTypes = async () => {
                const result = await getProjectTypes();
                const data = (result as DataResult<ProjectType[]>).data;
                setProjectTypes(data);
            };
            _setProjectTypes();
        }
    }, [projectTypes]);

    const _refresh = async () => {
        let result = await getProjectTypes();
        let data = (result as DataResult<ProjectType[]>).data;
        setProjectTypes(data);
    }

    const _userCanCreateProjectTypes = () => props.user?.role === TeamCloudUserRole.Admin;

    const _commandBarItems = (): ICommandBarItemProps[] => [
        { key: 'refresh', text: 'Refresh', iconProps: { iconName: 'refresh' }, onClick: () => { _refresh() } },
        { key: 'newProjectType', text: 'New project type', iconProps: { iconName: 'NewTeamProject' }, onClick: () => { setNewProjectTypePanelOpen(true) }, disabled: !_userCanCreateProjectTypes() },
    ];

    const _centerCommandBarItems: ICommandBarItemProps[] = [
        { key: 'search', onRender: () => <SearchBox className="searchBox" iconProps={{ iconName: 'Filter' }} placeholder="Filter" onChange={(_, filter) => setProjectTypeFilter(filter)} /> }
    ];

    const _breadcrumbs: IBreadcrumbItem[] = [
        { text: '', key: 'root', href: '/', isCurrentItem: true }
    ];

    const _onProjectTypeSelected = (projectType: ProjectType) => {
        setSelectedProjectType(projectType)
        setDetailsPanelOpen(true)
    };

    const _onDetailsPanelClose = () => {
        setSelectedProjectType(undefined)
        setDetailsPanelOpen(false)
    };

    return (
        <>
            <Stack>
                <SubheaderBar
                    breadcrumbs={_breadcrumbs}
                    commandBarItems={_commandBarItems()}
                    centerCommandBarItems={_centerCommandBarItems} />
                <ProjectTypeList
                    projectTypes={projectTypes}
                    projectTypeFilter={projectTypeFilter}
                    onProjectTypeSelected={_onProjectTypeSelected} />
            </Stack>
            <ProjectTypePanel
                projectType={selectedProjectType}
                panelIsOpen={detailsPanelOpen}
                onPanelClose={_onDetailsPanelClose} />
            <ProjectTypeForm
                panelIsOpen={newProjectTypePanelOpen}
                onFormClose={() => setNewProjectTypePanelOpen(false)} />
        </>
    );
}

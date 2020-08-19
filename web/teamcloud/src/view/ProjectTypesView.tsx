// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from "react";
import { ICommandBarItemProps, SearchBox, Stack, IBreadcrumbItem, Panel, PrimaryButton, DefaultButton, Spinner, Text } from '@fluentui/react';
import { getProjectTypes } from '../API'
import { Project, DataResult, ProjectType, User, TeamCloudUserRole } from '../model'
import { SubheaderBar, ProjectTypeForm, ProjectTypeList } from "../components";

export interface IProjectTypesViewProps {
    user?: User;
    onProjectSelected?: (project: Project) => void;
}

export const ProjectTypesView: React.FunctionComponent<IProjectTypesViewProps> = (props) => {

    const [projectTypes, setProjectTypes] = useState<ProjectType[]>();
    const [projectTypeFilter, setProjectTypeFilter] = useState<string>();
    const [newProjectTypePanelOpen, setNewProjectTypePanelOpen] = useState(false);

    const [newProjectFormEnabled] = useState<boolean>(true);
    const [newProjectTypeFormEnabled, setNewProjectTypeFormEnabled] = useState<boolean>(true);
    const [newProjectName] = useState<string>();
    const [newProjectType] = useState<ProjectType>();
    const [newProjectErrorText] = useState<string>();


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

    const _onProjectTypeFormNameChange = () => {
        // setNewProjectName(val);
    }

    const _onCreateNewProjectType = () => {

    }

    const _onNewProjectTypeFormReset = () => {
        setNewProjectTypePanelOpen(false);
        setNewProjectTypeFormEnabled(true);
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

    const _onRenderNewProjectTypeFormFooterContent = () => (
        <div>
            <PrimaryButton disabled={!newProjectFormEnabled || !(newProjectName && newProjectType)} onClick={() => _onCreateNewProjectType()} styles={{ root: { marginRight: 8 } }}>
                Create project type
            </PrimaryButton>
            <DefaultButton disabled={!newProjectFormEnabled} onClick={() => _onNewProjectTypeFormReset()}>Cancel</DefaultButton>
            <Spinner styles={{ root: { visibility: newProjectFormEnabled ? 'hidden' : 'visible' } }} />
        </div>
    );

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
                // onProjectTypeSelected={props.onProjectSelected}
                />
            </Stack>
            <Panel
                headerText='New project type'
                isOpen={newProjectTypePanelOpen}
                onDismiss={() => _onNewProjectTypeFormReset()}
                onRenderFooterContent={_onRenderNewProjectTypeFormFooterContent}>
                <ProjectTypeForm
                    fieldsEnabled={!newProjectTypeFormEnabled}
                    onNameChange={_onProjectTypeFormNameChange}
                    // onProjectTypeChange={_onProjectFormTypeChange}
                    onFormSubmit={() => _onCreateNewProjectType()} />
                <Text>{newProjectErrorText}</Text>
            </Panel>
        </>
    );
}

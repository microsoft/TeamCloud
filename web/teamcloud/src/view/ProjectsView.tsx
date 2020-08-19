// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from "react";
import { ICommandBarItemProps, SearchBox, Stack, IBreadcrumbItem, Panel, PrimaryButton, DefaultButton, Spinner, Text } from '@fluentui/react';
import { getProjects, createProject } from '../API'
import { Project, DataResult, ProjectType, User, ProjectDefinition, ProjectUserRole, StatusResult, ErrorResult, TeamCloudUserRole } from '../model'
import { ProjectList, ProjectForm, SubheaderBar } from "../components";

export interface IProjectsViewProps {
    user?: User;
    onProjectSelected?: (project: Project) => void;
}

export const ProjectsView: React.FunctionComponent<IProjectsViewProps> = (props) => {

    const [projects, setProjects] = useState<Project[]>();
    const [projectFilter, setProjectFilter] = useState<string>();
    const [newProjectPanelOpen, setNewProjectPanelOpen] = useState(false);

    const [newProjectFormEnabled, setNewProjectFormEnabled] = useState<boolean>(true);
    const [newProjectName, setNewProjectName] = useState<string>();
    const [newProjectType, setNewProjectType] = useState<ProjectType>();
    const [newProjectErrorText, setNewProjectErrorText] = useState<string>();


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


    const _onProjectFormNameChange = (val: string | undefined) => {
        setNewProjectName(val);
    }

    const _onProjectFormTypeChange = (val: ProjectType | undefined) => {
        setNewProjectType(val);
    }

    const _onCreateNewProject = async () => {
        setNewProjectFormEnabled(false);
        if (props.user && newProjectName && newProjectType) {
            const projectDefinition: ProjectDefinition = {
                name: newProjectName,
                projectType: newProjectType.id,
                users: [
                    {
                        identifier: props.user.id,
                        role: ProjectUserRole.Owner
                    }
                ]
            };
            const result = await createProject(projectDefinition);
            if ((result as StatusResult).code === 202)
                _onNewProjectFormReset();
            else if ((result as ErrorResult).errors) {
                // console.log(JSON.stringify(result));
                setNewProjectErrorText((result as ErrorResult).status);
            }
        }
    }

    const _onNewProjectFormReset = () => {
        setNewProjectPanelOpen(false);
        setNewProjectName(undefined);
        setNewProjectType(undefined);
        setNewProjectFormEnabled(true);
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

    const _onRenderNewProjectFormFooterContent = () => (
        <div>
            <PrimaryButton disabled={!newProjectFormEnabled || !(newProjectName && newProjectType)} onClick={() => _onCreateNewProject()} styles={{ root: { marginRight: 8 } }}>
                Create project
            </PrimaryButton>
            <DefaultButton disabled={!newProjectFormEnabled} onClick={() => _onNewProjectFormReset()}>Cancel</DefaultButton>
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
                <ProjectList
                    projects={projects}
                    projectFilter={projectFilter}
                    onProjectSelected={props.onProjectSelected} />
            </Stack>
            <Panel
                headerText='New project'
                isOpen={newProjectPanelOpen}
                onDismiss={() => _onNewProjectFormReset()}
                onRenderFooterContent={_onRenderNewProjectFormFooterContent}>
                <ProjectForm
                    fieldsEnabled={!newProjectFormEnabled}
                    onNameChange={_onProjectFormNameChange}
                    onProjectTypeChange={_onProjectFormTypeChange}
                    onFormSubmit={() => _onCreateNewProject()} />
                <Text>{newProjectErrorText}</Text>
            </Panel>
        </>
    );
}

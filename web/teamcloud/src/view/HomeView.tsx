import React, { useState, useEffect } from "react";
import { ICommandBarItemProps, SearchBox, Stack, IBreadcrumbItem, Panel, PrimaryButton, DefaultButton, Spinner, Text } from '@fluentui/react';
import { getProjects, createProject } from '../API'
import { Project, DataResult, ProjectType, User, ProjectDefinition, ProjectUserRole, StatusResult, ErrorResult, TeamCloudUserRole } from '../model'
import { ProjectList, ProjectForm, SubheaderBar } from "../components";

export interface IHomeViewProps {
    user?: User;
    onProjectSelected?: (project: Project) => void;
}

export const HomeView: React.FunctionComponent<IHomeViewProps> = (props) => {

    const [projects, setProjects] = useState<Project[]>();
    const [projectFilter, setProjectFilter] = useState<string>();
    const [panelOpen, setPanelOpen] = useState(false);

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

    const handleSubmit = async () => {
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
                onFormCancel();
            else if ((result as ErrorResult).errors) {
                // console.log(JSON.stringify(result));
                setNewProjectErrorText((result as ErrorResult).status);
            }
        }
    }

    const onFormCancel = () => {
        setPanelOpen(false);
        setNewProjectName(undefined);
        setNewProjectType(undefined);
    }

    const _userCanCreateProjects = () => props.user?.role === TeamCloudUserRole.Admin || props.user?.role === TeamCloudUserRole.Creator;

    const _commandBarItems = (): ICommandBarItemProps[] => [
        { key: 'refresh', text: 'Refresh', iconProps: { iconName: 'refresh' }, onClick: () => { _refresh() } },
        { key: 'newProject', text: 'New project', iconProps: { iconName: 'NewTeamProject' }, onClick: () => { setPanelOpen(true) }, disabled: !_userCanCreateProjects() },
    ];

    const _centerCommandBarItems: ICommandBarItemProps[] = [
        { key: 'search', onRender: () => <SearchBox className="searchBox" iconProps={{ iconName: 'Filter' }} placeholder="Filter" onChange={(_, filter) => setProjectFilter(filter)} /> }
    ];

    const _breadcrumbs: IBreadcrumbItem[] = [
        { text: 'Projects', key: 'projects', href: '/', isCurrentItem: true }
    ];

    const _onRenderPanelFooterContent = () => (
        <div>
            <PrimaryButton disabled={!newProjectFormEnabled || !(newProjectName && newProjectType)} onClick={() => handleSubmit()} styles={{ root: { marginRight: 8 } }}>
                Create project
            </PrimaryButton>
            <DefaultButton disabled={!newProjectFormEnabled} onClick={() => onFormCancel()}>Cancel</DefaultButton>
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
                isOpen={panelOpen}
                onDismiss={() => onFormCancel()}
                onRenderFooterContent={_onRenderPanelFooterContent}>
                <ProjectForm
                    fieldsEnabled={!newProjectFormEnabled}
                    onNameChange={_onProjectFormNameChange}
                    onProjectTypeChange={_onProjectFormTypeChange}
                    onFormSubmit={() => handleSubmit()} />
                <Text>{newProjectErrorText}</Text>
            </Panel>
        </>
    );
}

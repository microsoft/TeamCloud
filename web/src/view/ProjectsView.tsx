// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { ICommandBarItemProps, SearchBox, Stack, IBreadcrumbItem, Text, DefaultButton, PrimaryButton, Pivot, PivotItem } from '@fluentui/react';
// import { useParams } from 'react-router-dom';
import { Organization, Project, User } from 'teamcloud'
import { ProjectList, ProjectForm, SubheaderBar } from '../components';
import { api } from '../API';
import { useParams } from 'react-router-dom';

export interface IProjectsViewProps {
    // org?: Organization
    // orgId: string;
    user?: User;
    onProjectSelected?: (project: Project) => void;
}

export const ProjectsView: React.FunctionComponent<IProjectsViewProps> = (props) => {

    const [org, setOrg] = useState<Organization>();
    const [projects, setProjects] = useState<Project[]>();
    const [projectFilter, setProjectFilter] = useState<string>();
    const [newProjectPanelOpen, setNewProjectPanelOpen] = useState(false);

    let { orgId } = useParams() as { orgId: string };

    useEffect(() => {
        if (org === undefined) {
            const _setOrg = async () => {
                const result = await api.getOrganization(orgId);
                setOrg(result.data ?? undefined);
            };
            _setOrg();
        }
    }, [org, orgId]);

    useEffect(() => {
        if (org && projects === undefined) {
            const _setProjects = async () => {
                const result = await api.getProjects(org.id);
                setProjects(result.data ?? undefined);
            };
            _setProjects();
        }
    }, [org, projects]);

    const _refresh = async () => {
        if (org) {
            let result = await api.getProjects(org.id);
            setProjects(result.data ?? undefined);
        } else {
            setProjects(undefined);
        }
    }

    const _userCanCreateProjects = () => props.user?.role === 'Admin' || props.user?.role === 'Creator';

    const _commandBarItems = (): ICommandBarItemProps[] => [
        { key: 'refresh', text: 'Refresh', iconProps: { iconName: 'refresh' }, onClick: () => { _refresh() } },
        { key: 'newProject', text: 'New project', iconProps: { iconName: 'NewTeamProject' }, onClick: () => { setNewProjectPanelOpen(true) }, disabled: !_userCanCreateProjects() }
    ];

    const _centerCommandBarItems: ICommandBarItemProps[] = [
        { key: 'search', onRender: () => <SearchBox className='searchBox' iconProps={{ iconName: 'Filter' }} placeholder='Filter' onChange={(_, filter) => setProjectFilter(filter)} /> }
    ];

    const _breadcrumbs: IBreadcrumbItem[] = [
        { text: '', key: 'root', href: '/', isCurrentItem: true }
    ];

    return (
        <>
            <Stack>
                <Stack.Item styles={{ root: { padding: '24px 20px 0px 32px' } }}>
                    <Stack horizontal
                        verticalFill
                        horizontalAlign='space-between'
                        verticalAlign='baseline'>
                        <Stack.Item>
                            <Text variant='xLarge' >{org?.displayName}</Text>
                        </Stack.Item>
                        <Stack.Item>
                            <PrimaryButton iconProps={{ iconName: 'Add' }} text='New project' />
                        </Stack.Item>
                    </Stack>
                </Stack.Item>
                <Stack.Item styles={{ root: { padding: '8px 32px 16px 24px' } }}>
                    <Pivot>
                        <PivotItem headerText='Projects'>
                            <Stack styles={{ root: { paddingTop: '24px' } }}>
                                <ProjectList
                                    org={org}
                                    projects={projects}
                                    projectFilter={projectFilter}
                                    onProjectSelected={props.onProjectSelected} />
                            </Stack>
                        </PivotItem>
                        <PivotItem headerText='Templates'>
                            <Text>Templates</Text>
                        </PivotItem>
                        <PivotItem headerText='Components'>
                            <Text>Components</Text>
                        </PivotItem>
                    </Pivot>
                </Stack.Item>
            </Stack>
            <ProjectForm
                org={org}
                user={props.user}
                panelIsOpen={newProjectPanelOpen}
                onFormClose={() => setNewProjectPanelOpen(false)} />
        </>
    );
}

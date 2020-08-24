// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Project, User, DataResult, ProjectUserRole } from '../model';
import { getProject } from '../API';
import { Stack, Spinner, IBreadcrumbItem, ICommandBarItemProps } from '@fluentui/react';
import { IProjectViewDetailProps, ProjectViewDetail, SubheaderBar, UserForm, ProjectMembers } from '../components';
import { ProjectLinks } from '../components/ProjectLinks';

export interface IProjectDetailViewProps {
    user?: User;
    project?: Project;
    projectId: string;
}

export const ProjectDetailView: React.FunctionComponent<IProjectDetailViewProps> = (props) => {

    const [project, setProject] = useState(props.project);
    const [newUsersPanelOpen, setNewUsersPanelOpen] = useState(false);

    useEffect(() => {
        if (project === undefined) {
            const _setProject = async () => {
                const result = await getProject(props.projectId);
                const data = (result as DataResult<Project>).data;
                setProject(data);
            };
            _setProject();
        }
    }, [project, props.projectId]);

    const _refresh = async () => {
        let result = await getProject(project?.id ?? props.projectId);
        let data = (result as DataResult<Project>).data;
        setProject(data);
    };

    const _userIsProjectOwner = () =>
        props.user?.projectMemberships?.find(m => m.projectId === project?.id ?? props.projectId)?.role === ProjectUserRole.Owner;

    const _commandBarItems = (): ICommandBarItemProps[] => [
        { key: 'refresh', text: 'Refresh', iconProps: { iconName: 'Refresh' }, onClick: () => { _refresh() } },
        { key: 'addUser', text: 'Add users', iconProps: { iconName: 'PeopleAdd' }, onClick: () => { setNewUsersPanelOpen(true) }, disabled: !_userIsProjectOwner() },
    ];

    const _breadcrumbs: IBreadcrumbItem[] = [{ text: '', key: 'root', href: '/' }];

    const _ensureBreadcrumb = () => {
        if (project && _breadcrumbs.length === 1)
            _breadcrumbs.push({ text: project.name, key: 'project', isCurrentItem: true })
    };

    const projectDetailStackProps = (): IProjectViewDetailProps[] => {
        if (!project) return [];

        let _projectDetailStackProps: IProjectViewDetailProps[] = [
            {
                title: 'Project', details: [
                    { label: 'ID', value: project.id },
                    { label: 'Name', value: project.name }
                ]
            }, {
                title: 'Project Type', details: [
                    { label: 'ID', value: project.type.id },
                    { label: 'Default', value: project.type.isDefault ? 'Yes' : 'No' },
                    { label: 'Location', value: project.type.region },
                    { label: 'Providers', value: project.type.providers.map(p => p.id).join(', ') },
                    { label: 'Subscription Capacity', value: project.type.subscriptionCapacity.toString() },
                    { label: 'Subscriptions', value: project.type.subscriptions.join(', ') },
                    { label: 'Resource Group Name Prefix', value: project.type.resourceGroupNamePrefix ?? '' },
                ]
            }, {
                title: 'Resource Group', details: [
                    { label: 'Name', value: project.resourceGroup?.name },
                    { label: 'Location', value: project.resourceGroup?.region },
                    { label: 'Subscription', value: project.resourceGroup?.subscriptionId },
                ]
            }
        ];
        return _projectDetailStackProps;
    };

    const projectDetailStack = (projectDetailStackProps: IProjectViewDetailProps[]) =>
        projectDetailStackProps.map(p => <ProjectViewDetail key={p.title} title={p.title} details={p.details} />);


    if (project?.id) {
        _ensureBreadcrumb();

        return (
            <>
                <Stack>
                    <SubheaderBar
                        breadcrumbs={_breadcrumbs}
                        commandBarItems={_commandBarItems()}
                        breadcrumbsWidth='300px' />
                    <Stack
                        wrap
                        horizontal
                        styles={{ root: { padding: '0 24px' } }}
                        horizontalAlign='center'
                        verticalAlign='start'>
                        <Stack.Item grow styles={{ root: { minWidth: '367px', marginRight: '16px' } }}>
                            <ProjectMembers project={project} />
                            {projectDetailStack(projectDetailStackProps())}
                        </Stack.Item>
                        <Stack.Item grow styles={{ root: { minWidth: '367px', marginRight: '16px' } }}>
                            <ProjectLinks project={project} />
                        </Stack.Item>
                    </Stack>
                </Stack>
                <UserForm
                    project={project}
                    panelIsOpen={newUsersPanelOpen}
                    onFormClose={() => setNewUsersPanelOpen(false)} />
            </>
        );
    }

    return (<Stack verticalFill verticalAlign='center' horizontalAlign='center'><Spinner /></Stack>);
}

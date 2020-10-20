// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Stack, Spinner, IBreadcrumbItem, ICommandBarItemProps } from '@fluentui/react';
import { IProjectViewDetailProps, ProjectViewDetail, SubheaderBar, ProjectMembersForm, ProjectMembers, ProjectMemberForm, ProjectLinks, ProjectComponents } from '../components';
import { Project, User } from 'teamcloud';
import { ProjectMember } from '../model';
import { api } from '../API';

export interface IProjectDetailViewProps {
    user?: User;
    project?: Project;
    projectId: string;
}

export const ProjectDetailView: React.FunctionComponent<IProjectDetailViewProps> = (props) => {

    const [project, setProject] = useState(props.project);
    const [selectedMember, setSelectedMember] = useState<ProjectMember>();
    const [newUsersPanelOpen, setNewUsersPanelOpen] = useState(false);
    const [editUsersPanelOpen, setEditUsersPanelOpen] = useState(false);

    useEffect(() => {
        if (project === undefined) {
            const _setProject = async () => {
                const result = await api.getProjectByNameOrId(props.projectId);
                setProject(result.data);
            };
            _setProject();
        }
    }, [project, props.projectId]);

    const _refresh = async () => {
        let result = await api.getProjectByNameOrId(project?.id ?? props.projectId);
        setProject(result.data);
    };

    // const _userIsProjectOwner = () =>
    //     props.user?.projectMemberships?.find(m => m.projectId === project?.id ?? props.projectId)?.role === 'Owner';

    const _commandBarItems = (): ICommandBarItemProps[] => [
        { key: 'refresh', text: 'Refresh', iconProps: { iconName: 'Refresh' }, onClick: () => { _refresh() } },
        // { key: 'addUser', text: 'Add users', iconProps: { iconName: 'PeopleAdd' }, onClick: () => { setNewUsersPanelOpen(true) }, disabled: !_userIsProjectOwner() },
    ];

    const _breadcrumbs: IBreadcrumbItem[] = [{ text: '', key: 'root', href: '/' }];

    const _ensureBreadcrumb = () => {
        if (project && _breadcrumbs.length === 1)
            _breadcrumbs.push({ text: project.name ?? '', key: 'project', isCurrentItem: true })
    };


    const projectDetailProjectStackProps = () => {
        if (!project) return null;
        const detailProps: IProjectViewDetailProps = {
            title: 'Project', details: [
                { label: 'ID', value: project.id ?? '' },
                { label: 'Name', value: project.name }
            ]
        };
        return (<ProjectViewDetail key={detailProps.title} title={detailProps.title} details={detailProps.details} />);
    };

    const projectDetailProjectTypeStackProps = () => {
        if (!project || !project.type) return null;
        const detailProps: IProjectViewDetailProps = {
            title: 'Project Type', details: [
                { label: 'ID', value: project.type.id ?? '' },
                { label: 'Default', value: project.type.isDefault ? 'Yes' : 'No' },
                { label: 'Location', value: project.type.region ?? '' },
                { label: 'Providers', value: project.type.providers?.map(p => p.id).join(', ') ?? '' },
                { label: 'Subscription Capacity', value: project.type.subscriptionCapacity?.toString() ?? '' },
                { label: 'Subscriptions', value: project.type.subscriptions?.join(', ') ?? '' },
                { label: 'Resource Group Name Prefix', value: project.type.resourceGroupNamePrefix ?? '' },
            ]
        };
        return (<ProjectViewDetail key={detailProps.title} title={detailProps.title} details={detailProps.details} />);
    };

    const projectDetailResourceGroupStackProps = () => {
        if (!project || !project.resourceGroup) return null;
        const detailProps: IProjectViewDetailProps = {
            title: 'Resource Group', details: [
                { label: 'Name', value: project.resourceGroup.name ?? '' },
                { label: 'Location', value: project.resourceGroup.region ?? '' },
                { label: 'Subscription', value: project.resourceGroup.subscriptionId ?? '' },
            ]
        };
        return (<ProjectViewDetail key={detailProps.title} title={detailProps.title} details={detailProps.details} />);
    };

    const _onEditMember = (member?: ProjectMember) => {
        setSelectedMember(member)
        setEditUsersPanelOpen(true)
    };


    if (project?.id) {
        _ensureBreadcrumb();

        return (
            <>
                <Stack>
                    <SubheaderBar
                        breadcrumbs={_breadcrumbs}
                        commandBarItems={_commandBarItems()}
                        breadcrumbsWidth='300px'
                        commandBarWidth='90px' />
                    <Stack
                        wrap
                        horizontal
                        styles={{ root: { padding: '0 24px' } }}
                        horizontalAlign='center'
                        verticalAlign='start'>
                        {/* <Stack.Item grow styles={{ root: { minWidth: '367px', marginRight: '16px' } }}> */}
                        <Stack.Item grow styles={{ root: { minWidth: '40%', marginRight: '16px' } }}>
                            <ProjectMembers
                                user={props.user}
                                project={project}
                                onEditMember={_onEditMember} />
                            <ProjectComponents user={props.user} project={project} />
                            {projectDetailProjectTypeStackProps()}
                        </Stack.Item>
                        {/* <Stack.Item grow styles={{ root: { minWidth: '367px', marginRight: '16px' } }}> */}
                        <Stack.Item grow styles={{ root: { minWidth: '40%', marginRight: '16px' } }}>
                            {projectDetailProjectStackProps()}
                            <ProjectLinks project={project} />
                            {projectDetailResourceGroupStackProps()}
                        </Stack.Item>
                        {/* {projectDetailStack(projectDetailStackProps())} */}
                    </Stack>
                </Stack>
                <ProjectMembersForm
                    project={project}
                    panelIsOpen={newUsersPanelOpen}
                    onFormClose={() => setNewUsersPanelOpen(false)} />
                <ProjectMemberForm
                    user={selectedMember?.user}
                    project={project}
                    graphUser={selectedMember?.graphUser}
                    panelIsOpen={editUsersPanelOpen}
                    onFormClose={() => { setEditUsersPanelOpen(false); setSelectedMember(undefined) }} />
            </>
        );
    }

    return (<Stack verticalFill verticalAlign='center' horizontalAlign='center'><Spinner /></Stack>);
}

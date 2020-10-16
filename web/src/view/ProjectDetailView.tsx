// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Project, User, ProjectMembershipRole } from 'teamcloud';
import { ProjectMember, DataResult } from '../model';
import { getProject } from '../API';
import { Stack, Spinner, IBreadcrumbItem, ICommandBarItemProps } from '@fluentui/react';
import { IProjectViewDetailProps, ProjectViewDetail, SubheaderBar, ProjectMembersForm, ProjectMembers, ProjectMemberForm, ProjectLinks, ProjectComponents } from '../components';

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
            console.log('go')
            const _setProject = async () => {
                const result = await getProject(props.projectId);
                console.log(result)
                // const data = (result as DataResult<Project>).data;
                // console.log(data)
                if (result) {
                    console.log(result.links?.offers?.href ?? 'nope')
                    setProject(result);
                }
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

    // const projectDetailStackProps = (): IProjectViewDetailProps[] => {
    //     if (!project) return [];

    //     let _projectDetailStackProps: IProjectViewDetailProps[] = [
    //         {
    //             title: 'Project', details: [
    //                 { label: 'ID', value: project.id },
    //                 { label: 'Name', value: project.name }
    //             ]
    //         }, {
    //             title: 'Project Type', details: [
    //                 { label: 'ID', value: project.type.id },
    //                 { label: 'Default', value: project.type.isDefault ? 'Yes' : 'No' },
    //                 { label: 'Location', value: project.type.region },
    //                 { label: 'Providers', value: project.type.providers.map(p => p.id).join(', ') },
    //                 { label: 'Subscription Capacity', value: project.type.subscriptionCapacity.toString() },
    //                 { label: 'Subscriptions', value: project.type.subscriptions.join(', ') },
    //                 { label: 'Resource Group Name Prefix', value: project.type.resourceGroupNamePrefix ?? '' },
    //             ]
    //         }, {
    //             title: 'Resource Group', details: [
    //                 { label: 'Name', value: project.resourceGroup?.name },
    //                 { label: 'Location', value: project.resourceGroup?.region },
    //                 { label: 'Subscription', value: project.resourceGroup?.subscriptionId },
    //             ]
    //         }
    //     ];
    //     return _projectDetailStackProps;
    // };

    const projectDetailStack = (projectDetailStackProps: IProjectViewDetailProps[]) =>
        projectDetailStackProps.map(p => <Stack.Item grow styles={{ root: { minWidth: '40%', marginRight: '16px' } }}><ProjectViewDetail key={p.title} title={p.title} details={p.details} /></Stack.Item>);

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

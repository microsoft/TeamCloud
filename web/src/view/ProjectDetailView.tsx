// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Stack, Spinner, IBreadcrumbItem, Persona, PersonaSize, getTheme, IconButton, ProgressIndicator } from '@fluentui/react';
import { ProjectMembersForm, ProjectMembers, ProjectMemberForm, ProjectLinks, ProjectComponents } from '../components';
import { Project, User } from 'teamcloud';
import { ProjectMember } from '../model';
import { api } from '../API';
import { useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';

export interface IProjectDetailViewProps {
    user?: User;
    project?: Project;
    // orgId: string;
    // projectId: string;
}

export const ProjectDetailView: React.FunctionComponent<IProjectDetailViewProps> = (props) => {

    let isAuthenticated = useIsAuthenticated();
    let { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const [project, setProject] = useState(props.project);
    const [favorite, setFavorate] = useState(false);
    const [selectedMember, setSelectedMember] = useState<ProjectMember>();
    const [newUsersPanelOpen, setNewUsersPanelOpen] = useState(false);
    const [editUsersPanelOpen, setEditUsersPanelOpen] = useState(false);


    useEffect(() => {
        if (isAuthenticated && orgId && projectId) {

            if (project && (project.id === projectId || project.slug === projectId))
                return;

            setProject(undefined);

            const _setProject = async () => {
                const result = await api.getProject(projectId, orgId);
                setProject(result.data);
            };

            _setProject();
        }
    }, [isAuthenticated, project, projectId, orgId]);

    // const _refresh = async () => {
    //     let o = project?.organization ?? orgId;
    //     let p = project?.id ?? projectId;
    //     if (o && p) {
    //         let result = await api.getProject(p, o);
    //         setProject(result.data);
    //     }
    // };

    // const _userIsProjectOwner = () =>
    //     props.user?.projectMemberships?.find(m => m.projectId === project?.id ?? projectId)?.role === 'Owner';

    // const _commandBarItems = (): ICommandBarItemProps[] => [
    //     { key: 'refresh', text: 'Refresh', iconProps: { iconName: 'Refresh' }, onClick: () => { _refresh() } },
    //     // { key: 'addUser', text: 'Add users', iconProps: { iconName: 'PeopleAdd' }, onClick: () => { setNewUsersPanelOpen(true) }, disabled: !_userIsProjectOwner() },
    // ];

    // const _breadcrumbs: IBreadcrumbItem[] = [{ text: '', key: 'root', href: '/' }];

    // const _ensureBreadcrumb = () => {
    //     if (project && _breadcrumbs.length === 1)
    //         _breadcrumbs.push({ text: project.displayName ?? '', key: 'project', isCurrentItem: true })
    // };


    // const projectDetailProjectStackProps = () => {
    //     if (!project) return null;
    //     const detailProps: IProjectViewDetailProps = {
    //         title: 'Project', details: [
    //             { label: 'ID', value: project.id ?? '' },
    //             { label: 'Name', value: project.displayName }
    //         ]
    //     };
    //     return (<ProjectViewDetail key={detailProps.title} title={detailProps.title} details={detailProps.details} />);
    // };

    // const projectDetailProjectTemplateStackProps = () => {
    //     if (!project || !project.template) return null;
    //     const detailProps: IProjectViewDetailProps = {
    //         title: 'Project Type', details: [
    //             { label: 'ID', value: project.type.id ?? '' },
    //             { label: 'Default', value: project.type.isDefault ? 'Yes' : 'No' },
    //             { label: 'Location', value: project.type.region ?? '' },
    //             { label: 'Providers', value: project.type.providers?.map(p => p.id).join(', ') ?? '' },
    //             { label: 'Subscription Capacity', value: project.type.subscriptionCapacity?.toString() ?? '' },
    //             { label: 'Subscriptions', value: project.type.subscriptions?.join(', ') ?? '' },
    //             { label: 'Resource Group Name Prefix', value: project.type.resourceGroupNamePrefix ?? '' },
    //         ]
    //     };
    //     return (<ProjectViewDetail key={detailProps.title} title={detailProps.title} details={detailProps.details} />);
    // };

    // const projectDetailResourceGroupStackProps = () => {
    //     if (!project || !project.resourceGroup) return null;
    //     const detailProps: IProjectViewDetailProps = {
    //         title: 'Resource Group', details: [
    //             { label: 'Name', value: project.resourceGroup.name ?? '' },
    //             { label: 'Location', value: project.resourceGroup.region ?? '' },
    //             { label: 'Subscription', value: project.resourceGroup.subscriptionId ?? '' },
    //         ]
    //     };
    //     return (<ProjectViewDetail key={detailProps.title} title={detailProps.title} details={detailProps.details} />);
    // };

    const _onEditMember = (member?: ProjectMember) => {
        setSelectedMember(member)
        setEditUsersPanelOpen(true)
    };

    const theme = getTheme();

    if (project?.id) {
        // _ensureBreadcrumb();

        return (
            <>
                <Stack>
                    <Stack.Item styles={{ root: { margin: '0px', padding: '24px 30px 20px 32px', backgroundColor: theme.palette.white, borderBottom: `${theme.palette.neutralLight} solid 1px` } }}>
                        <Stack horizontal
                            verticalFill
                            horizontalAlign='space-between'
                            verticalAlign='baseline'>
                            <Stack.Item>
                                <Persona
                                    text={project.displayName}
                                    size={PersonaSize.size48}
                                    coinProps={{ styles: { initials: { borderRadius: '4px', fontSize: '20px', fontWeight: '400' } } }}
                                    styles={{ primaryText: { fontSize: theme.fonts.xxLarge.fontSize, fontWeight: '700', letterSpacing: '-1.12px', marginLeft: '20px' } }}
                                    imageInitials={project.displayName.split(' ').map(s => s[0].toUpperCase()).join('')} />
                            </Stack.Item>
                            <Stack.Item>
                                <IconButton
                                    toggle
                                    checked={favorite}
                                    onClick={() => setFavorate(!favorite)}
                                    iconProps={{ iconName: favorite ? 'FavoriteStarFill' : 'FavoriteStar', color: 'yellow' }} />
                                {/* <PrimaryButton iconProps={{ iconName: 'Add' }} text='New project' /> */}
                            </Stack.Item>
                        </Stack>
                    </Stack.Item>
                    <Stack.Item styles={{ root: { padding: '24px' } }}>
                        <Stack
                            wrap
                            horizontal
                            horizontalAlign='center'
                            verticalAlign='start'>
                            {/* <Stack.Item grow styles={{ root: { minWidth: '367px', marginRight: '16px' } }}> */}
                            {/* <Stack.Item grow styles={{ root: { minWidth: '367px', marginRight: '16px' } }}> */}
                            <Stack.Item grow styles={{ root: { minWidth: '60%', marginRight: '16px' } }}>
                                <ProjectComponents project={project} />
                                {/* {projectDetailProjectStackProps()} */}
                                {/* {projectDetailResourceGroupStackProps()} */}
                            </Stack.Item>
                            {/* {projectDetailStack(projectDetailStackProps())} */}
                            <Stack.Item grow styles={{ root: { minWidth: '20%', marginRight: '16px' } }}>
                                <ProjectMembers
                                    user={props.user}
                                    project={project}
                                    onEditMember={_onEditMember} />
                                <ProjectLinks project={project} />
                                {/* {projectDetailProjectTemplateStackProps()} */}
                            </Stack.Item>
                        </Stack>
                    </Stack.Item>
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

    return (<ProgressIndicator progressHidden={project !== undefined} styles={{ itemProgress: { padding: '0px', marginTop: '-2px' } }} />);
}

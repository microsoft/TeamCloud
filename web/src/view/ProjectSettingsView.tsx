// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Route, useParams } from 'react-router-dom';
import { Stack, Persona, PersonaSize, getTheme, IconButton, ProgressIndicator } from '@fluentui/react';
import { useIsAuthenticated } from '@azure/msal-react';
import { Project, User } from 'teamcloud';
import { ProjectMembersForm, ProjectMemberForm, ProjectMembers, ProjectComponents, ProjectOverview, ProjectSettingsMembers, ProjectSettingsComponents, ProjectSettingsOverview, ContentHeader, ContentContainer, ContentProgress } from '../components';
import { ProjectMember } from '../model';
import { api } from '../API';

export interface IProjectSettingsViewProps {
    user?: User;
    project?: Project;
    // orgId: string;
    // projectId: string;
}

export const ProjectSettingsView: React.FunctionComponent<IProjectSettingsViewProps> = (props) => {

    let isAuthenticated = useIsAuthenticated();
    let { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const [user, setUser] = useState(props.user);
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
                    <ContentHeader title={project.displayName}>
                        <IconButton
                            toggle
                            checked={favorite}
                            onClick={() => setFavorate(!favorite)}
                            iconProps={{ iconName: favorite ? 'FavoriteStarFill' : 'FavoriteStar', color: 'yellow' }} />
                    </ContentHeader>
                    <ContentContainer>
                        <Route exact path='/orgs/:orgId/projects/:projectId/settings'>
                            <ProjectSettingsOverview {...{ project: project, user: user }} />
                        </Route>
                        <Route exact path='/orgs/:orgId/projects/:projectId/settings/components'>
                            <ProjectSettingsComponents {...{ project: project, user: user }} />
                        </Route>
                        <Route exact path='/orgs/:orgId/projects/:projectId/settings/members'>
                            <ProjectSettingsMembers {...{ project: project, user: user, onEditMember: _onEditMember }} />
                        </Route>
                    </ContentContainer>
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

    return (<ContentProgress progressHidden={project !== undefined} />);
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Project, User, DataResult, ProjectUserRole, UserDefinition, StatusResult, ErrorResult } from '../model';
import { getProject, createProjectUser } from '../API';
import { Stack, Spinner, IBreadcrumbItem, ICommandBarItemProps, PrimaryButton, DefaultButton, Panel, Text, IStackStyles } from '@fluentui/react';
import { IProjectViewDetailProps, ProjectViewDetail, SubheaderBar, UserForm, ProjectMembers } from '../components';
import { ProjectLinks } from '../components/ProjectLinks';

export interface IProjectViewProps {
    user?: User;
    project?: Project;
    projectId: string;
}

export const ProjectView: React.FunctionComponent<IProjectViewProps> = (props) => {

    const [project, setProject] = useState(props.project);
    const [panelOpen, setPanelOpen] = useState(false);

    const [newUserFormEnabled, setNewUserFormEnabled] = useState<boolean>(true);
    const [newUserIdentifiers, setNewUserIdentifiers] = useState<string[]>();
    const [newUserRole, setNewUserRole] = useState<ProjectUserRole>();
    const [newUserErrorText, setNewUserErrorText] = useState<string>();

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
    }

    const _userIsProjectOwner = () =>
        props.user?.projectMemberships?.find(m => m.projectId === project?.id ?? props.projectId)?.role === ProjectUserRole.Owner;

    const _commandBarItems = (): ICommandBarItemProps[] => [
        { key: 'refresh', text: 'Refresh', iconProps: { iconName: 'Refresh' }, onClick: () => { _refresh() } },
        { key: 'addUser', text: 'Add users', iconProps: { iconName: 'PeopleAdd' }, onClick: () => { setPanelOpen(true) }, disabled: !_userIsProjectOwner() },
    ];

    const _breadcrumbs: IBreadcrumbItem[] = [{ text: '', key: 'root', href: '/' }];

    const _ensureBreadcrumb = () => {
        if (project && _breadcrumbs.length === 1)
            _breadcrumbs.push({ text: project.name, key: 'project', isCurrentItem: true })
    }

    const _onUserFormIdentifiersChange = (val?: string[]) => {
        setNewUserIdentifiers(val);
    }

    const _onUserFormRoleChange = (val?: ProjectUserRole) => {
        setNewUserRole(val);
    }

    const _onNewUsersFormSubmit = async () => {
        setNewUserFormEnabled(false);
        if (project && newUserRole && _hasNewUserIdentifers()) {
            const userDefinitions: UserDefinition[] = newUserIdentifiers!.map(i => ({
                identifier: i,
                role: newUserRole
            }));
            const results = await Promise
                .all(userDefinitions.map(async d => await createProjectUser(project.id, d)));

            let errors: ErrorResult[] = [];
            results.forEach(r => {
                if ((r as StatusResult).code !== 202 && (r as ErrorResult).errors)
                    errors.push((r as ErrorResult));
            });
            if (errors.length > 0) {
                errors.forEach(e => console.log(e));
                setNewUserErrorText(`The following errors occured: ${errors.map(e => e.status).join()}`);
            } else {
                _resetUserFormAndDismiss();
            }
        }
    }

    const _resetUserFormAndDismiss = () => {
        setPanelOpen(false);
        setNewUserRole(undefined);
        setNewUserIdentifiers(undefined);
        setNewUserFormEnabled(true);
    }

    const _hasNewUserIdentifers = () => (newUserIdentifiers?.length && newUserIdentifiers.length > 0);

    const _onRenderPanelFooterContent = () => (
        <div>
            <PrimaryButton disabled={!newUserFormEnabled || !(newUserRole && _hasNewUserIdentifers())} onClick={() => _onNewUsersFormSubmit()} styles={{ root: { marginRight: 8 } }}>
                Add users
            </PrimaryButton>
            <DefaultButton disabled={!newUserFormEnabled} onClick={() => _resetUserFormAndDismiss()}>Cancel</DefaultButton>
            <Spinner styles={{ root: { visibility: newUserFormEnabled ? 'hidden' : 'visible' } }} />
        </div>
    );

    const leftProjectDetailStackProps = (): IProjectViewDetailProps[] => {
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
            }
            , {
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

    const _detailStackStyles: IStackStyles = {
        root: {
            padding: '0 24px',
        }
    }

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
                        styles={_detailStackStyles}
                        horizontalAlign='center'
                        verticalAlign='start'>
                        <Stack.Item grow styles={{ root: { minWidth: '367px', marginRight: '16px' } }}>
                            <ProjectMembers project={project} />
                            {projectDetailStack(leftProjectDetailStackProps())}
                        </Stack.Item>
                        <Stack.Item grow styles={{ root: { minWidth: '367px', marginRight: '16px' } }}>
                            <ProjectLinks project={project} />
                        </Stack.Item>
                    </Stack>
                </Stack>
                <Panel
                    headerText='Add Users'
                    isOpen={panelOpen}
                    onDismiss={() => _resetUserFormAndDismiss()}
                    onRenderFooterContent={_onRenderPanelFooterContent}>
                    <UserForm
                        fieldsEnabled={!newUserFormEnabled}
                        onUserIdentifiersChange={_onUserFormIdentifiersChange}
                        onUserRoleChange={_onUserFormRoleChange}
                        onFormSubmit={() => _onNewUsersFormSubmit()} />
                    <Text>{newUserErrorText}</Text>
                </Panel>
            </>
        );
    }
    return (<Stack verticalFill verticalAlign='center' horizontalAlign='center'><Spinner /></Stack>);
}

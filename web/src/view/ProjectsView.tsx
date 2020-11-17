// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { Stack, Text, PrimaryButton, Pivot, PivotItem, ProgressIndicator } from '@fluentui/react';
import { Organization, Project } from 'teamcloud'
import { ProjectList, ProjectForm } from '../components';
import { api } from '../API';
import { useIsAuthenticated } from '@azure/msal-react';

export interface IProjectsViewProps {
    // org?: Organization
    // orgId: string;
    // user?: User;
    onProjectSelected?: (project: Project) => void;
}

export const ProjectsView: React.FunctionComponent<IProjectsViewProps> = (props: IProjectsViewProps) => {

    let history = useHistory();
    let { orgId } = useParams() as { orgId: string };

    let isAuthenticated = useIsAuthenticated();

    const [org, setOrg] = useState<Organization>();
    const [projects, setProjects] = useState<Project[]>();

    useEffect(() => {
        if (isAuthenticated || orgId) {

            if (org && (org.id === orgId || org.slug === orgId))
                return;

            setOrg(undefined);
            setProjects(undefined);

            const _setOrg = async () => {

                const promises: any[] = [
                    api.getOrganization(orgId),
                    api.getProjects(orgId)
                ];

                var results = await Promise.all(promises);

                setOrg(results[0]?.data ?? undefined);
                setProjects(results[1].data ?? []);
            };

            _setOrg();
        }
    }, [isAuthenticated, orgId, org, projects]);

    // useEffect(() => {
    //     if (org && projects === undefined) {
    //         // console.error('getProjects');
    //         const _setProjects = async () => {
    //             const result = await api.getProjects(org.id);
    //             setProjects(result.data ?? undefined);
    //         };
    //         _setProjects();
    //     }
    // }, [org, projects]);


    // const _userCanCreateProjects = () => props.user?.role === 'Admin' || props.user?.role === 'Creator';

    // const _commandBarItems = (): ICommandBarItemProps[] => [
    //     { key: 'refresh', text: 'Refresh', iconProps: { iconName: 'refresh' }, onClick: () => { _refresh() } },
    //     { key: 'newProject', text: 'New project', iconProps: { iconName: 'NewTeamProject' }, onClick: () => { setNewProjectPanelOpen(true) }, disabled: !_userCanCreateProjects() }
    // ];

    // const _centerCommandBarItems: ICommandBarItemProps[] = [
    //     { key: 'search', onRender: () => <SearchBox className='searchBox' iconProps={{ iconName: 'Filter' }} placeholder='Filter' onChange={(_, filter) => setProjectFilter(filter)} /> }
    // ];

    // const _breadcrumbs: IBreadcrumbItem[] = [
    //     { text: '', key: 'root', href: '/', isCurrentItem: true }
    // ];

    return (
        <Stack>
            <ProgressIndicator
                progressHidden={org !== undefined && projects !== undefined}
                styles={{ itemProgress: { padding: '0px', marginTop: '-2px' } }} />
            <Stack.Item styles={{ root: { padding: '24px 30px 0px 32px' } }}>
                <Stack horizontal
                    verticalFill
                    horizontalAlign='space-between'
                    verticalAlign='baseline'>
                    <Stack.Item>
                        <Text variant='xLarge' >{org?.displayName}</Text>
                    </Stack.Item>
                    <Stack.Item>
                        <PrimaryButton
                            disabled={org == undefined}
                            iconProps={{ iconName: 'Add' }} text='New project' onClick={() => history.push(`/orgs/${orgId}/projects/new`)} />
                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item styles={{ root: { padding: '8px 32px 16px 24px' } }}>
                <Pivot>
                    <PivotItem headerText='Projects'>
                        <Stack styles={{ root: { paddingTop: '24px' } }}>
                            <ProjectList
                                projects={projects}
                                onProjectSelected={props.onProjectSelected} />
                        </Stack>
                    </PivotItem>
                    {/* <PivotItem headerText='Templates'>
                        <Text>Templates</Text>
                    </PivotItem>
                    <PivotItem headerText='Components'>
                        <Text>Components</Text>
                    </PivotItem> */}
                </Pivot>
            </Stack.Item>
        </Stack>
    );
}

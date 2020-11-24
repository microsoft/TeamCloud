// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Stack, getTheme } from '@fluentui/react';
import { Project, User } from 'teamcloud';
import { ProjectMember } from '../model';
import { ProjectOverviewMembers, ProjectLinks, ProjectOverviewComponents } from '../components';

export interface IProjectOverviewProps {
    user?: User;
    project?: Project;
    // orgId: string;
    // projectId: string;
}

export const ProjectOverview: React.FunctionComponent<IProjectOverviewProps> = (props) => {

    // let isAuthenticated = useIsAuthenticated();
    // let { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const [selectedMember, setSelectedMember] = useState<ProjectMember>();
    const [editUsersPanelOpen, setEditUsersPanelOpen] = useState(false);

    const _onEditMember = (member?: ProjectMember) => {
        setSelectedMember(member)
        setEditUsersPanelOpen(true)
    };

    const theme = getTheme();

    return props.project?.id ? (
        <Stack
            wrap
            horizontal
            horizontalAlign='center'
            verticalAlign='start'>
            {/* <Stack.Item grow styles={{ root: { minWidth: '367px', marginRight: '16px' } }}> */}
            {/* <Stack.Item grow styles={{ root: { minWidth: '367px', marginRight: '16px' } }}> */}
            <Stack.Item grow styles={{ root: { minWidth: '60%', marginRight: '16px' } }}>
                <ProjectOverviewComponents project={props.project} />
                {/* {projectDetailProjectStackProps()} */}
                {/* {projectDetailResourceGroupStackProps()} */}
            </Stack.Item>
            {/* {projectDetailStack(projectDetailStackProps())} */}
            <Stack.Item grow styles={{ root: { minWidth: '20%', marginRight: '16px' } }}>
                <ProjectOverviewMembers
                    user={props.user}
                    project={props.project}
                    onEditMember={_onEditMember} />
                <ProjectLinks project={props.project} />
                {/* {projectDetailProjectTemplateStackProps()} */}
            </Stack.Item>
        </Stack>
    ) : <></>;
}

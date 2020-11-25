// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Stack, getTheme } from '@fluentui/react';
import { Project, User, Component } from 'teamcloud';
import { ProjectMember } from '../model';
import { ProjectOverviewMembers, ProjectLinks, ProjectOverviewComponents } from '../components';

export interface IProjectOverviewProps {
    user?: User;
    project?: Project;
    members?: ProjectMember[];
    components?: Component[];
}

export const ProjectOverview: React.FunctionComponent<IProjectOverviewProps> = (props) => {

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
                <ProjectOverviewComponents
                    project={props.project}
                    components={props.components} />
                {/* {projectDetailProjectStackProps()} */}
                {/* {projectDetailResourceGroupStackProps()} */}
            </Stack.Item>
            {/* {projectDetailStack(projectDetailStackProps())} */}
            <Stack.Item grow styles={{ root: { minWidth: '20%', marginRight: '16px' } }}>
                <ProjectOverviewMembers
                    user={props.user}
                    project={props.project}
                    members={props.members}
                    onEditMember={_onEditMember} />
                <ProjectLinks project={props.project} />
                {/* {projectDetailProjectTemplateStackProps()} */}
            </Stack.Item>
        </Stack>
    ) : <></>;
}

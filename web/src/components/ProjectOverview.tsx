// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Stack } from '@fluentui/react';
import { Project, User, Component } from 'teamcloud';
import { Member, ProjectMember } from '../model';
import { MembersCard, ComponentsCard } from '../components';

export interface IProjectOverviewProps {
    user?: User;
    project?: Project;
    members?: ProjectMember[];
    components?: Component[];
}

export const ProjectOverview: React.FC<IProjectOverviewProps> = (props) => {

    const _onEditMember = (member?: Member) => {
        // setSelectedMember(member as ProjectMember)
        // setEditUsersPanelOpen(true)
    };

    return (
        <Stack
            wrap
            horizontal
            horizontalAlign='center'
            verticalAlign='start'>
            <Stack.Item grow styles={{ root: { minWidth: '60%', marginRight: '16px' } }}>
                <ComponentsCard
                    project={props.project}
                    components={props.components} />
            </Stack.Item>
            <Stack.Item grow styles={{ root: { minWidth: '20%', marginRight: '16px' } }}>
                <MembersCard
                    user={props.user}
                    project={props.project}
                    members={props.members}
                    onEditMember={_onEditMember} />
            </Stack.Item>
        </Stack>
    );
}

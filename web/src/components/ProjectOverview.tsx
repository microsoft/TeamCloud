// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Stack } from '@fluentui/react';
import { MembersCard, ComponentsCard } from '../components';
import { useProject } from '../Hooks';

export const ProjectOverview: React.FC = () => {

    const { members, addUsers, removeUsers } = useProject();

    return (
        <Stack
            wrap
            horizontal
            horizontalAlign='center'
            verticalAlign='start'>
            <Stack.Item styles={{ root: { minWidth: '60%', marginRight: '16px' } }}>
                <ComponentsCard />
            </Stack.Item>
            <Stack.Item grow styles={{ root: { minWidth: '20%', marginRight: '16px' } }}>
                <MembersCard members={members} addUsers={addUsers} removeUsers={removeUsers} />
            </Stack.Item>
        </Stack>
    );
}

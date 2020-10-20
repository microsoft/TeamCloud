// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Stack, Text, Label } from '@fluentui/react';
import { ProjectDetailCard } from '.';

export interface IProjectViewDetailProps {
    title: string;
    details: { label: string, value: string }[]
}

export const ProjectViewDetail: React.FunctionComponent<IProjectViewDetailProps> = (props) => {

    const _getDetailStacks = () => props.details.map(d => (
        <Stack
            horizontal
            verticalAlign='baseline'
            key={d.label + d.value}
            tokens={{ childrenGap: 10 }}>
            <Label>{d.label}:</Label>
            <Text>{d.value}</Text>
        </Stack>
    ));

    return (
        <ProjectDetailCard title={props.title}>
            {_getDetailStacks()}
        </ProjectDetailCard>
    );
}

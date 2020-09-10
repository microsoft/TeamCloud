// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Panel, Stack, Label, Text, ITextStyles, FontWeights } from '@fluentui/react';
import { ProjectType } from '../model';


export interface IProjectTypePanelProps {
    projectType?: ProjectType;
    panelIsOpen: boolean;
    onPanelClose: () => void;
}

export const ProjectTypePanel: React.FunctionComponent<IProjectTypePanelProps> = (props) => {
    const _titleStyles: ITextStyles = {
        root: {
            fontSize: '21px',
            fontWeight: FontWeights.semibold,
        }
    }

    const _getDetailStacks = () => {

        let sections = [
            {
                title: 'Project Type', details: [
                    { label: 'ID', value: props.projectType?.id },
                    { label: 'Default', value: props.projectType?.isDefault ? 'Yes' : 'No' },
                    { label: 'Location', value: props.projectType?.region },
                    { label: 'Providers', value: props.projectType?.providers.map(p => p.id).join(', ') },
                    { label: 'Subscription Capacity', value: props.projectType?.subscriptionCapacity.toString() },
                    { label: 'Subscriptions', value: props.projectType?.subscriptions.join(', ') },
                    { label: 'Resource Group Name Prefix', value: props.projectType?.resourceGroupNamePrefix ?? '' },
                ]
            }
        ]

        return sections.map(s => {
            let details = s.details.map(d => (
                <Stack
                    verticalAlign='baseline'
                    key={d.label + d.value}>
                    <Label>{d.label}:</Label>
                    <Text>{d.value}</Text>
                </Stack>
            ));
            return (
                <Stack tokens={{ childrenGap: 10 }}>
                    <Text styles={_titleStyles}>{s.title}</Text>
                    {details}
                </Stack>
            )
        })
    };


    return (
        <Panel
            isOpen={props.panelIsOpen}
            onDismiss={() => props.onPanelClose()}>
            <Stack
                tokens={{ childrenGap: 30 }}>
                {_getDetailStacks()}
            </Stack>
        </Panel>
    );
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Panel, Stack, Label, Text, ITextStyles, FontWeights } from '@fluentui/react';
import { Provider } from 'teamcloud';

export interface IProviderPanelProps {
    provider?: Provider;
    panelIsOpen: boolean;
    onPanelClose: () => void;
}

export const ProviderPanel: React.FunctionComponent<IProviderPanelProps> = (props) => {

    const _titleStyles: ITextStyles = {
        root: {
            fontSize: '21px',
            fontWeight: FontWeights.semibold,
        }
    }

    const _getDetailStacks = () => {

        let sections = [
            {
                title: 'Provider', details: [
                    { label: 'ID', value: props.provider?.id },
                    { label: 'Type', value: props.provider?.type },
                    { label: 'Version', value: props.provider?.version },
                    { label: 'Url', value: props.provider?.url },
                    { label: 'Principal Id', value: props.provider?.principalId },
                    { label: 'Registered', value: props.provider?.registered?.toDateString() },
                    { label: 'Command Mode', value: props.provider?.commandMode },
                ]
            },
            {
                title: 'Resource Group', details: [
                    { label: 'Name', value: props.provider?.resourceGroup?.name },
                    { label: 'Location', value: props.provider?.resourceGroup?.region },
                    { label: 'Subscription', value: props.provider?.resourceGroup?.subscriptionId },
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
                <Stack
                    tokens={{ childrenGap: 10 }}
                    key={s.title}>
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

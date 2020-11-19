// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Stack, Text, Label, ITextStyles, FontWeights } from '@fluentui/react';

export interface IOrgSettingsDetailProps {
    title: string;
    details: { label: string, value?: string, required?: boolean }[]
}

export const OrgSettingsDetail: React.FunctionComponent<IOrgSettingsDetailProps> = (props) => {

    const _titleStyles: ITextStyles = {
        root: {
            fontSize: '21px',
            fontWeight: FontWeights.semibold,
            marginBottom: '12px'
        }
    }

    const _getTitle = (): JSX.Element | null => props.title ? <Text styles={_titleStyles}>{props.title}</Text> : null;

    const _getDetailStacks = () => props.details.map(d => (
        <Stack
            horizontal
            verticalAlign='baseline'
            key={`${props.title}${d.label}`}
            tokens={{ childrenGap: 10 }}>
            <Label required={d.required}>{d.label}:</Label>
            <Text>{d.value}</Text>
        </Stack>
    ));

    return (

        <Stack>
            {_getTitle()}
            {_getDetailStacks()}
        </Stack>
    );
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { FontWeights, getTheme, IStackStyles, ITextStyles, Stack, Text } from '@fluentui/react';

export interface ICalloutLabelProps {
    title?: string;
    callout?: string | number;
    large?: boolean;
    styles?: IStackStyles;
    titleStyles?: ITextStyles;
    calloutStyles?: ITextStyles;
    calloutBackground?: string;
}

export const CalloutLabel: React.FunctionComponent<ICalloutLabelProps> = (props) => {

    const theme = getTheme();

    const _titleStyles: ITextStyles = {
        root: (props.large ?? false)
            ? {
                fontSize: '21px',
                fontWeight: FontWeights.semibold,
                marginBottom: '12px'
            }
            : {
                fontSize: '14px',
                fontWeight: FontWeights.regular,

            }
    };

    const _calloutStyles: ITextStyles = {
        root: (props.large ?? false)
            ? {
                fontSize: '13px',
                fontWeight: FontWeights.regular,
                color: 'rgb(102, 102, 102)',
                backgroundColor: theme.palette.neutralLighter,
                marginBottom: '14px',
                marginTop: '5px',
                padding: '2px 12px',
                borderRadius: '14px',
            }
            : {
                fontSize: '11px',
                fontWeight: FontWeights.regular,
                color: 'rgb(102, 102, 102)',
                backgroundColor: props.calloutBackground ?? theme.palette.neutralLighter,
                padding: '2px 9px',
                borderRadius: '14px',
            }
    };

    const _getTitle = (): JSX.Element | null => props.title ? <Text styles={_titleStyles}>{props.title}</Text> : null;

    const _getCallout = (): JSX.Element | null => props.callout ? <Text styles={_calloutStyles}>{props.callout}</Text> : null;


    return (
        <Stack horizontal styles={props.styles} verticalFill verticalAlign='baseline' tokens={{ childrenGap: '5px' }}>
            <Stack.Item>
                {_getTitle()}
            </Stack.Item>
            <Stack.Item>
                {_getCallout()}
            </Stack.Item>
        </Stack>
    );
}

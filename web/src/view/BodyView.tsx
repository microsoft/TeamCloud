// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { getTheme, Stack } from '@fluentui/react';
import React from 'react';

export interface IBodyViewProps {
    nav: React.ReactNode,
    content: React.ReactNode
}

export const BodyView: React.FC<IBodyViewProps> = (props) => {
    const theme = getTheme();

    const leftStackStyles = {
        root: {
            width: '260px',
            paddingTop: '20px',
            paddingBottom: '10px',
            borderRight: `${theme.palette.neutralLight} solid 1px`
        }
    };

    const rightStackStyles = {
        root: {
            backgroundColor: theme.palette.neutralLighterAlt
        }
    };

    return (
        <Stack horizontal disableShrink verticalFill verticalAlign='stretch'>
            <Stack.Item styles={leftStackStyles}>
                {props.nav}
            </Stack.Item>
            <Stack.Item grow styles={rightStackStyles}>
                {props.content}
            </Stack.Item>
        </Stack>
    );
}

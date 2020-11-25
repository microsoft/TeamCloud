// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { getTheme, Image, PrimaryButton, Stack, Text } from '@fluentui/react';
import React, { useState, useEffect } from 'react';

export interface INoDataProps {
    title?: string;
    description?: string;
    image?: string;
    buttonText?: string;
    buttonIcon?: string;
    onButtonClick?: () => void;
}

export const NoData: React.FunctionComponent<INoDataProps> = (props) => {

    const theme = getTheme();

    return (
        <Stack
            horizontalAlign='center'
            // tokens={{ childrenGap: '12px' }}
            styles={{ root: { height: '100%' } }}>
            <Stack.Item>
                <Image
                    height='360px'
                    src={props.image} />
            </Stack.Item>
            {props.title && (
                <Stack.Item styles={{ root: { paddingBottom: '12px' } }}>
                    <Text
                        styles={{ root: { fontSize: '26px', fontWeight: '700', letterSpacing: '-1.12px' } }}>
                        {props.title}
                    </Text>
                </Stack.Item>
            )}
            {props.description && (
                <Stack.Item styles={{ root: { paddingBottom: '12px' } }}>
                    <Text
                        styles={{ root: {} }}>
                        {props.description}
                    </Text>
                </Stack.Item>
            )}
            {props.buttonText && (
                <Stack.Item>
                    <PrimaryButton
                        text={props.buttonText}
                        iconProps={props.buttonIcon ? { iconName: props.buttonIcon } : undefined}
                        onClick={props.onButtonClick} />
                </Stack.Item>
            )}
        </Stack>
    );
}

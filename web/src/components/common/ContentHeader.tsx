// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { getTheme, Persona, PersonaSize, Stack, Text } from '@fluentui/react';
import React from 'react';

export interface IContentHeaderProps {
    wide?: boolean;
    coin?: boolean;
    title?: string;
}

export const ContentHeader: React.FC<IContentHeaderProps> = (props) => {

    const theme = getTheme();

    return (
        <Stack.Item styles={{ root: { margin: '0px', padding: '24px 30px 20px 32px', backgroundColor: theme.palette.white, borderBottom: `${theme.palette.neutralLight} solid 1px` } }}>
            <Stack horizontal
                verticalFill
                horizontalAlign='space-between'
                verticalAlign='baseline'>
                <Stack.Item>
                    {props.title && (props.coin ?? false) && (
                        <Persona
                            text={props.title}
                            size={PersonaSize.size48}
                            coinProps={{ styles: { initials: { borderRadius: '4px', fontSize: '20px', fontWeight: '400' } } }}
                            styles={{ primaryText: { fontSize: theme.fonts.xxLarge.fontSize, fontWeight: '700', letterSpacing: '-1.12px', marginLeft: '20px' } }} />
                    )}
                    {props.title && !(props.coin ?? false) && (
                        <Text
                            styles={{ root: { textTransform: 'capitalize', fontSize: theme.fonts.xxLarge.fontSize, fontWeight: '700', letterSpacing: '-1.12px', marginLeft: props.wide ? '12px' : '0px' } }}>
                            {props.title}
                        </Text>
                    )}
                </Stack.Item>
                <Stack.Item>
                    {props.children}
                </Stack.Item>
            </Stack>
        </Stack.Item>
    );
}

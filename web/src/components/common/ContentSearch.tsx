// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { SearchBox, Stack, getTheme } from '@fluentui/react';

export interface IContentSearchProps {
    placeholder?: string;
    onChange?: (event?: React.ChangeEvent<HTMLInputElement>, newValue?: string) => void;
}

export const ContentSearch: React.FC<IContentSearchProps> = (props) => {

    const theme = getTheme();

    return (
        <Stack horizontal styles={{
            root: {
                padding: '10px 16px 10px 6px',
                borderRadius: theme.effects.roundedCorner4,
                boxShadow: theme.effects.elevation4,
                backgroundColor: theme.palette.white
            }
        }} >
            <Stack.Item grow>
                <SearchBox
                    placeholder={props.placeholder ?? 'Filter items'}
                    iconProps={{ iconName: 'Filter' }}
                    onChange={props.onChange}
                    styles={{
                        root: {
                            border: 'none !important', selectors: {
                                '::after': { border: 'none !important' },
                                ':hover .ms-SearchBox-iconContainer': { color: theme.palette.neutralTertiary }
                            }
                        },
                        iconContainer: { color: theme.palette.neutralTertiary, },
                        field: { border: 'none !important' }
                    }} />
            </Stack.Item>
            <Stack.Item>
                {props.children}
            </Stack.Item>
        </Stack>
    );
}

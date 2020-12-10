// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Stack } from '@fluentui/react';

export interface IContentContainerProps {
    wide?: boolean;
    full?: boolean;
}

export const ContentContainer: React.FC<IContentContainerProps> = (props) => {

    return (
        <Stack.Item styles={{ root: { padding: props.wide ? '24px 44px' : '24px', height: props.full ? '100%' : 'auto', flexShrink: props.full ? 1 : 0 } }}>
            {props.children}
        </Stack.Item>
    );
}

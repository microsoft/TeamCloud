// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { FontIcon, Link, Stack, Text } from '@fluentui/react';
import { Component } from 'teamcloud';

export interface IComponentLinkProps {
    text?: string;
    component?: Component;
}

export const ComponentLink: React.FunctionComponent<IComponentLinkProps> = (props) => {

    const { text, component } = props;

    return component?.resourceUrl ? (

        <Stack horizontal tokens={{ childrenGap: '4px' }} >
            <Link
                target='_blank'
                href={component.resourceUrl}>
                { text ?? component?.type ?? 'Component' }
            </Link>
            <FontIcon iconName='NavigateExternalInline' className='component-link-icon' />
        </Stack>

    ) : (
    
        <Stack horizontal tokens={{ childrenGap: '4px' }} >
            <Text>{ text ?? component?.type ?? 'Component' }</Text>
        </Stack>

    );
}

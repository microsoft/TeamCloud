// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { FontIcon, Link, Stack } from '@fluentui/react';
import { Component } from 'teamcloud';
import { useOrg } from '../hooks';

export interface IComponentLinkProps {
    component?: Component;
}

export const ComponentLink: React.FunctionComponent<IComponentLinkProps> = (props) => {

    const { data: org } = useOrg();
    const { component } = props;

    return org && component?.resourceId ? (

        <Stack horizontal tokens={{ childrenGap: '4px' }} >
            <Link
                target='_blank'
                href={`https://portal.azure.com/#@${org?.tenant}/resource${component?.resourceId}`}>
                Azure Portal
            </Link>
            <FontIcon iconName='NavigateExternalInline' className='component-link-icon' />
        </Stack>

    ) : <></>;
}

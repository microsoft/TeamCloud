// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useContext } from 'react';
import { FontIcon, Link, Stack } from '@fluentui/react';
import { OrgContext } from '../Context';
import { Component} from 'teamcloud';

export interface IComponentLinkProps {
    component?: Component;
}

export const ComponentLink: React.FunctionComponent<IComponentLinkProps> = (props) => {

    const { org } = useContext(OrgContext);
    const { component } = props;

    return org && component ? (

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
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { FontIcon, Link, Stack } from '@fluentui/react';
import { ComponentTemplate } from 'teamcloud';
import { useOrg } from '../hooks';

export interface IComponentTemplateLinkProps {
    componentTemplate?: ComponentTemplate;
}

export const ComponentTemplateLink: React.FunctionComponent<IComponentTemplateLinkProps> = (props) => {

    const { data: org } = useOrg();

    const { componentTemplate } = props;

    return org && componentTemplate?.repository?.url ? (

        <Stack horizontal tokens={{ childrenGap: '4px' }} >
            <Link
                target='_blank'
                href={`${componentTemplate.repository.url}/tree/${componentTemplate.repository.version}/${componentTemplate.folder}`}>
                {componentTemplate.displayName}
            </Link>
            <FontIcon iconName='NavigateExternalInline' className='component-link-icon' />
        </Stack>

    ) : <></>;
}

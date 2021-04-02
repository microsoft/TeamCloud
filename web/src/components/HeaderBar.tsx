// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { useHistory } from 'react-router-dom';
import { Stack, getTheme, Link } from '@fluentui/react';
import { HeaderBreadcrumb, UserInfo } from '.';

export const HeaderBar: React.FC = () => {

    const history = useHistory();

    const theme = getTheme();

    return (
        <header>
            <Stack
                horizontal
                verticalFill
                verticalAlign='center'
                horizontalAlign='space-between'
                styles={{ root: { height: '48px', borderBottom: `${theme.palette.neutralLight} solid 1px` } }}>
                <Stack.Item>
                    <Stack horizontal verticalFill verticalAlign='center'>
                        <Stack.Item styles={{ root: { width: '260px' } }}>
                            <Link styles={{ root: { fontWeight: 'bold', paddingLeft: '12px', color: theme.palette.themePrimary, fontSize: theme.fonts.mediumPlus.fontSize } }} onClick={() => history.push('/')}>TeamCloud</Link>
                        </Stack.Item>
                        <Stack.Item styles={{ root: { paddingLeft: '12px' } }}>
                            <HeaderBreadcrumb />
                        </Stack.Item>
                    </Stack>
                </Stack.Item>
                <Stack.Item>
                    <UserInfo />
                </Stack.Item>
            </Stack>
        </header>
    );
}

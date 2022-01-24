// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Route, Routes, useNavigate } from 'react-router-dom';
import { Stack, getTheme, Link } from '@fluentui/react';
import { HeaderBreadcrumb, UserInfo } from '.';

export const HeaderBar: React.FC = () => {

    const navigate = useNavigate();

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
                            <Link styles={{ root: { fontWeight: 'bold', paddingLeft: '12px', color: theme.palette.themePrimary, fontSize: theme.fonts.mediumPlus.fontSize } }} onClick={() => navigate('/')}>TeamCloud</Link>
                        </Stack.Item>
                        <Stack.Item styles={{ root: { paddingLeft: '12px' } }}>
                            <Routes>
                                <Route path='' element={<HeaderBreadcrumb />} />
                                <Route path='orgs/:orgId/*' element={<HeaderBreadcrumb />} />
                                <Route path='orgs/:orgId/settings/:settingId/*' element={<HeaderBreadcrumb />} />
                                <Route path='orgs/:orgId/projects/:projectId/*' element={<HeaderBreadcrumb />} />
                                <Route path='orgs/:orgId/projects/:projectId/settings/*' element={<HeaderBreadcrumb />} />
                                <Route path='orgs/:orgId/projects/:projectId/settings/:settingId/*' element={<HeaderBreadcrumb />} />
                                <Route path='orgs/:orgId/projects/:projectId/settings/:settingId/:itemId/*' element={<HeaderBreadcrumb />} />
                                <Route path='orgs/:orgId/projects/:projectId/:navId/*' element={<HeaderBreadcrumb />} />
                                <Route path='orgs/:orgId/projects/:projectId/:navId/:itemId/*' element={<HeaderBreadcrumb />} />
                                <Route path='orgs/:orgId/projects/:projectId/:navId/:itemId/tasks/:subitemId/*' element={<HeaderBreadcrumb />} />
                            </Routes>
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

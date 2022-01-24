// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Nav, INavLinkGroup, Stack, getTheme } from '@fluentui/react';

export const OrgSettingsNav: React.FC = () => {

    const navigate = useNavigate();
    const { orgId, settingId } = useParams() as { orgId: string, settingId: string };

    const _navLinkGroups = (): INavLinkGroup[] => [{
        links: orgId ? [
            {
                key: 'overview',
                name: 'Overview',
                url: '',
                onClick: () => navigate(`/orgs/${orgId}/settings`),
                iconProps: { iconName: 'Settings' }
            },
            {
                key: 'members',
                name: 'Members',
                url: '',
                onClick: () => navigate(`/orgs/${orgId}/settings/members`),
                iconProps: { iconName: 'Group' }
            },
            // {
            //     key: 'configuration',
            //     name: 'Configuration',
            //     url: '',
            //     onClick: () => navigate(`/orgs/${orgId}/settings/configuration`),
            //     iconProps: { iconName: 'Processing' } // Repair
            // },
            {
                key: 'scopes',
                name: 'Deployment Scopes',
                url: '',
                onClick: () => navigate(`/orgs/${orgId}/settings/scopes`),
                iconProps: { iconName: 'ScopeTemplate' }
            },
            {
                key: 'templates',
                name: 'Project Templates',
                url: '',
                onClick: () => navigate(`/orgs/${orgId}/settings/templates`),
                iconProps: { iconName: 'Rocket' }
            },
            {
                key: 'auditing',
                name: 'Auditing',
                url: '',
                onClick: () => navigate(`/orgs/${orgId}/settings/audit`),
                iconProps: { iconName: 'WaitlistConfirm' } // MultiSelectMirrored, ActivateOrders, IssueTrackingMirrored
            },
            {
                key: 'usage',
                name: 'Usage',
                url: '',
                onClick: () => navigate(`/orgs/${orgId}/settings/usage`),
                iconProps: { iconName: 'BarChartVertical' } // BarChart4, BIDashboard
            },
            // {
            //     key: 'providers',
            //     name: 'Custom Providers',
            //     url: '',
            //     onClick: () => navigate(`/orgs/${orgId}/settings/providers`),
            //     iconProps: { iconName: 'WebAppBuilderFragment' }
            // }
        ] : []
    }];

    const theme = getTheme();

    return (
        <Stack
            verticalFill
            verticalAlign="space-between">
            <Stack.Item>
                <Nav
                    selectedKey={settingId ?? 'overview'}
                    groups={_navLinkGroups()}
                    styles={{ root: [{ width: '100%' }], link: { color: theme.palette.neutralDark, padding: '8px 4px 8px 12px' } }} />
            </Stack.Item>
            <Stack.Item>
            </Stack.Item>
        </Stack>
    );
}

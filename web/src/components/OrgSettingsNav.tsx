// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { Nav, INavLinkGroup, Stack, getTheme } from '@fluentui/react';

export interface IOrgSettingsNavProps { }

export const OrgSettingsNav: React.FC<IOrgSettingsNavProps> = (props) => {

    let { orgId, settingId } = useParams() as { orgId: string, settingId: string };

    const history = useHistory();

    const _navLinkGroups = (): INavLinkGroup[] => [{
        links: orgId ? [
            {
                key: 'overview',
                name: 'Overview',
                url: '',
                onClick: () => history.push(`/orgs/${orgId}/settings`),
                iconProps: { iconName: 'Settings' }
            },
            {
                key: 'members',
                name: 'Members',
                url: '',
                onClick: () => history.push(`/orgs/${orgId}/settings/members`),
                iconProps: { iconName: 'Group' }
            },
            // {
            //     key: 'configuration',
            //     name: 'Configuration',
            //     url: '',
            //     onClick: () => history.push(`/orgs/${orgId}/settings/configuration`),
            //     iconProps: { iconName: 'Processing' } // Repair
            // },
            {
                key: 'scopes',
                name: 'Deployment Scopes',
                url: '',
                onClick: () => history.push(`/orgs/${orgId}/settings/scopes`),
                iconProps: { iconName: 'ScopeTemplate' }
            },
            {
                key: 'templates',
                name: 'Project Templates',
                url: '',
                onClick: () => history.push(`/orgs/${orgId}/settings/templates`),
                iconProps: { iconName: 'Rocket' }
            },
            // {
            //     key: 'providers',
            //     name: 'Custom Providers',
            //     url: '',
            //     onClick: () => history.push(`/orgs/${orgId}/settings/providers`),
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
                {/* <ActionButton
                    disabled={orgId === undefined || projectId === undefined}
                    iconProps={{ iconName: 'Settings' }}
                    styles={{ root: { padding: '10px 8px 10px 12px' } }}
                    text={'Project settings'}
                    onClick={() => history.push(`/orgs/${orgId}/projects/${projectId}/settings`)} /> */}
            </Stack.Item>
        </Stack>
    );
}

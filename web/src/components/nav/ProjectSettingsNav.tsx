// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { Nav, INavLinkGroup, Stack, getTheme } from '@fluentui/react';

export interface IProjectSettingsNavProps { }

export const ProjectSettingsNav: React.FC<IProjectSettingsNavProps> = (props) => {

    const history = useHistory();
    const { orgId, projectId, settingId } = useParams() as { orgId: string, projectId: string, settingId: string };

    const _navLinkGroups = (): INavLinkGroup[] => [{
        links: (orgId && projectId) ? [
            {
                key: 'overview',
                name: 'Overview',
                url: '',
                onClick: () => history.push(`/orgs/${orgId}/projects/${projectId}/settings`),
                iconProps: { iconName: 'Settings' }
            },
            {
                key: 'schedules',
                name: 'Schedules',
                url: '',
                onClick: () => history.push(`/orgs/${orgId}/projects/${projectId}/settings/schedules`),
                iconProps: { iconName: 'ScheduleEventAction' }
            }
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

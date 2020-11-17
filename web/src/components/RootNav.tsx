// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { Nav, INavLinkGroup, INavLink, Stack, ActionButton, Persona, PersonaSize, getTheme } from '@fluentui/react';
import { Organization } from 'teamcloud'
import { api } from '../API';

export interface IRootNavProps { }

export const RootNav: React.FunctionComponent<IRootNavProps> = (props) => {

    let { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    const history = useHistory();

    const isAuthenticated = useIsAuthenticated();

    const [orgs, setOrgs] = useState<Organization[]>();

    const newOrgView = orgId !== undefined && orgId.toLowerCase() === 'new';

    useEffect(() => {
        if (isAuthenticated && orgs === undefined) {
            // console.error('getOrganizations');
            const _setOrgs = async () => {
                const result = await api.getOrganizations();
                setOrgs(result.data ?? undefined);
            };
            _setOrgs();
        }
    }, [isAuthenticated, orgs]);

    const _navLinkGroups = (): INavLinkGroup[] => {
        const links: INavLink[] = orgs?.map(o => ({
            key: o.slug,
            name: o.displayName,
            url: '',
            onClick: () => history.push(`/orgs/${o.slug}`),
        })) ?? [];

        if (!newOrgView)
            links.push({
                key: 'new',
                name: "New organization",
                url: '',
                onClick: () => history.push('/orgs/new')
            });

        return [{ links: links }];
    };

    const _projectLinkGroups = (): INavLinkGroup[] => [{
        links: (orgId && projectId) ? [
            {
                key: 'overview',
                name: 'Overview',
                url: '',
                onClick: () => history.push(`/orgs/${orgId}/projects/${projectId}`),
            },
            {
                key: 'components',
                name: 'Components',
                url: '',
                onClick: () => history.push(`/orgs/${orgId}/projects/${projectId}/components`),
            },
            {
                key: 'members',
                name: 'Members',
                url: '',
                onClick: () => history.push(`/orgs/${orgId}/projects/${projectId}/members`),
            },
        ] : []
    }];

    const theme = getTheme();

    function _onRenderLink(link?: INavLink): JSX.Element {
        if (link?.key && link.key === 'new')
            return <ActionButton text={link.name}
                styles={{ root: { color: theme.palette.themePrimary, padding: '0px' } }} />
        else
            return <Persona
                text={link?.name}
                size={PersonaSize.size24}
                coinProps={{ styles: { initials: { borderRadius: '4px' } } }}
                imageInitials={link?.name[0].toUpperCase()} />;
    };

    return (
        <Stack
            verticalFill
            verticalAlign="space-between">
            <Stack.Item>
                <Nav
                    selectedKey={orgId}
                    groups={_navLinkGroups()}
                    onRenderLink={_onRenderLink}
                    styles={{ root: [{ width: '100%' }], link: { padding: '8px 4px 8px 12px' } }} />
            </Stack.Item>
            <Stack.Item>
                <ActionButton
                    disabled={newOrgView || orgId === undefined}
                    iconProps={{ iconName: 'Settings' }}
                    styles={{ root: { padding: '10px 8px 10px 12px' } }}
                    text={'Organization settings'}
                    onClick={() => history.push(`/orgs/${orgId}/settings`)} />
            </Stack.Item>
        </Stack>
    );
}

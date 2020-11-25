// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { useIsAuthenticated } from '@azure/msal-react';
import { Nav, INavLinkGroup, INavLink, Stack, ActionButton, Persona, PersonaSize, getTheme, Text } from '@fluentui/react';
import { Organization } from 'teamcloud'
import { api } from '../API';

export interface IRootNavProps { }

export const RootNav: React.FC<IRootNavProps> = (props) => {

    let { orgId } = useParams() as { orgId: string };

    let history = useHistory();

    let isAuthenticated = useIsAuthenticated();

    const [orgs, setOrgs] = useState<Organization[]>();

    const newOrgView = orgId !== undefined && orgId.toLowerCase() === 'new';

    useEffect(() => {
        if (isAuthenticated && (orgs === undefined || (orgId && orgId !== 'new' && !orgs.some(o => o.id === orgId || o.slug === orgId)))) {
            const _setOrgs = async () => {
                const result = await api.getOrganizations();
                setOrgs(result.data ?? undefined);
                if (result.code === 200 && result.data) {
                    if (result.data.length === 0)
                        history.push('/orgs/new');
                    else if (history.location.pathname === '/' && result.data.length === 1)
                        history.push(`/orgs/${result.data[0].slug}`);
                }
            };
            _setOrgs();
        }
    }, [isAuthenticated, history, orgId, orgs]);

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

    const theme = getTheme();

    const _onRenderLink = (link?: INavLink): JSX.Element => (link?.key && link.key === 'new')
        ? <Text styles={{ root: { color: theme.palette.themePrimary, padding: '0px' } }}>{link.name}</Text>
        : <Persona
            text={link?.name}
            size={PersonaSize.size24}
            coinProps={{ styles: { initials: { borderRadius: '4px' } } }} />

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
                {!newOrgView && orgId !== undefined && (<ActionButton
                    disabled={newOrgView || orgId === undefined}
                    iconProps={{ iconName: 'Settings' }}
                    styles={{ root: { padding: '10px 8px 10px 12px' } }}
                    text={'Organization settings'}
                    onClick={() => history.push(`/orgs/${orgId}/settings`)} />)}
            </Stack.Item>
        </Stack>
    );
}

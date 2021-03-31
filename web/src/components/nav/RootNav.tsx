// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { Nav, INavLinkGroup, INavLink, Stack, ActionButton, Persona, PersonaSize, getTheme, Text } from '@fluentui/react';
import { useOrgs } from '../../Hooks';

export const RootNav: React.FC = () => {

    const history = useHistory();
    const { orgId } = useParams() as { orgId: string };

    const { orgs, onOrgSelected } = useOrgs();

    const newOrgView = orgId !== undefined && orgId.toLowerCase() === 'new';

    const _navLinkGroups = (): INavLinkGroup[] => {
        const links: INavLink[] = orgs?.map(o => ({
            key: o.slug,
            name: o.displayName,
            url: '',
            onClick: () => {
                onOrgSelected(o);
                history.push(`/orgs/${o.slug}`)
            },
        })) ?? [];

        if (!newOrgView)
            links.push({
                key: 'new',
                name: "New organization",
                url: '',
                onClick: () => {
                    onOrgSelected(undefined);
                    history.push('/orgs/new')
                }
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

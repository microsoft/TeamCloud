// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect } from 'react';
import { useHistory, useParams } from 'react-router-dom';
// import { useIsAuthenticated } from '@azure/msal-react';
import { Nav, INavLinkGroup, INavLink, Stack, ActionButton, Persona, PersonaSize, getTheme, Text } from '@fluentui/react';
import { Organization } from 'teamcloud'
// import { api } from '../API';

export interface IRootNavProps {
    org?: Organization;
    orgs?: Organization[];
    onOrgSelected: (org?: Organization) => void;
}

export const RootNav: React.FC<IRootNavProps> = (props) => {

    // const isAuthenticated = useIsAuthenticated();

    const history = useHistory();
    const { orgId } = useParams() as { orgId: string };

    // const [orgs, setOrgs] = useState<Organization[]>();

    const newOrgView = orgId !== undefined && orgId.toLowerCase() === 'new';

    useEffect(() => {
        if (orgId && orgId.toLowerCase() !== 'new') {
            if (props.org && (props.org.id.toLowerCase() === orgId.toLowerCase() || props.org.slug.toLowerCase() === orgId.toLowerCase())) {
                return;
            } else if (props.orgs) {
                const find = props.orgs.find(o => o.id.toLowerCase() === orgId.toLowerCase() || o.slug.toLowerCase() === orgId.toLowerCase());
                if (find) {
                    console.log(`setOrg (${orgId})`);
                    props.onOrgSelected(find);
                }
            }
        }
    }, [orgId, props]);

    const _navLinkGroups = (): INavLinkGroup[] => {
        const links: INavLink[] = props.orgs?.map(o => ({
            key: o.slug,
            name: o.displayName,
            url: '',
            onClick: () => {
                props.onOrgSelected(o);
                history.push(`/orgs/${o.slug}`)
            },
        })) ?? [];

        if (!newOrgView)
            links.push({
                key: 'new',
                name: "New organization",
                url: '',
                onClick: () => {
                    props.onOrgSelected(undefined);
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

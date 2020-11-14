// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState } from 'react';
import { Nav, INavLinkGroup, INavLink, Stack, ActionButton, Persona, PersonaSize, Text, getTheme } from '@fluentui/react';
import { Organization, Project } from 'teamcloud'
import { useParams } from 'react-router-dom';

export interface IRootNavProps {
    // org?: Organization;
    orgs?: Organization[];
    // orgId?: string;
    // projectId?: string;
    // project?: Project;
    onOrgSelected: (org?: Organization) => void;
}

export const RootNav: React.FunctionComponent<IRootNavProps> = (props) => {

    let { orgId, projectId } = useParams() as { orgId: string, projectId: string };

    // const { orgs } = props;

    // const [org, setOrg] = useState<Organization>();

    const _onOrgSelected = (o: Organization) => {
        console.error(orgId);
        console.error(projectId);
        // setOrg(o);
        props.onOrgSelected(o);
    }

    const _navLinkGroups = (): INavLinkGroup[] => (orgId && projectId) ? _projectLinkGroups() : _orgsLinkGroups();

    const _orgsLinkGroups = (): INavLinkGroup[] => {
        const links: INavLink[] = props.orgs?.map(o => ({
            key: o.slug,
            name: o.displayName,
            url: `/orgs/${o.slug}`,
            onClick: () => _onOrgSelected(o),
        })) ?? [];

        links.push({ key: 'new', name: "New organization", url: '#' })

        return [{ links: links }];
    };

    const _projectLinkGroups = (): INavLinkGroup[] => [{
        links: (orgId && projectId) ? [
            {
                key: 'overview',
                name: 'Overview',
                url: `/orgs/${orgId}/projects/${projectId}`,
                onClick: () => { },
            },
            {
                key: 'components',
                name: 'Components',
                url: `/orgs/${orgId}/projects/${projectId}/components`,
                onClick: () => { },
            },
            {
                key: 'members',
                name: 'Members',
                url: `/orgs/${orgId}/projects/${projectId}/members`,
                onClick: () => { },
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
                    // selectedKey={projectId ? 'overview' : orgId}
                    groups={_navLinkGroups()}
                    onRenderLink={_onRenderLink}
                    styles={{ root: [{ width: '100%' }], link: { padding: '8px 4px 8px 12px' } }} />
            </Stack.Item>
            <Stack.Item>
                <ActionButton
                    iconProps={{ iconName: 'Settings' }}
                    styles={{ root: { padding: '10px 8px 10px 12px' } }}
                    text='Organization settings' />
            </Stack.Item>
        </Stack>
    );
}

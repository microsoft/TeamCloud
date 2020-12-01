// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect } from 'react';
import { useHistory, useParams } from 'react-router-dom';
import { Nav, INavLinkGroup, INavLink, Stack, ActionButton, Persona, PersonaSize } from '@fluentui/react';
import { Organization, Project } from 'teamcloud';

export interface IProjectNavProps {
    org?: Organization;
    orgs?: Organization[];
    project?: Project;
    projects?: Project[];
    onOrgSelected: (org?: Organization) => void;
    onProjectSelected: (project?: Project) => void;
}

export const ProjectNav: React.FC<IProjectNavProps> = (props) => {

    const history = useHistory();
    const { orgId, projectId, navId } = useParams() as { orgId: string, projectId: string, navId: string };

    useEffect(() => {
        if (orgId) {
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

    useEffect(() => {
        if (projectId) {
            if (props.project && (props.project.id.toLowerCase() === projectId.toLowerCase() || props.project.slug.toLowerCase() === projectId.toLowerCase())) {
                return;
            } else if (props.projects) {
                const find = props.projects.find(p => p.id.toLowerCase() === projectId.toLowerCase() || p.slug.toLowerCase() === projectId.toLowerCase());
                if (find) {
                    console.log(`setProject (${projectId})`);
                    props.onProjectSelected(find);
                }
            }
        }
    }, [projectId, props]);

    const _navLinkGroups = (): INavLinkGroup[] => [{
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

    const _onRenderLink = (link?: INavLink): JSX.Element => <Persona
        text={link?.name}
        size={PersonaSize.size24}
        coinProps={{ styles: { initials: { borderRadius: '4px' } } }} />

    return (
        <Stack
            verticalFill
            verticalAlign="space-between">
            <Stack.Item>
                <Nav
                    selectedKey={navId ?? 'overview'}
                    groups={_navLinkGroups()}
                    onRenderLink={_onRenderLink}
                    styles={{ root: [{ width: '100%' }], link: { padding: '8px 4px 8px 12px' } }} />
            </Stack.Item>
            <Stack.Item>
                <ActionButton
                    disabled={orgId === undefined || projectId === undefined}
                    iconProps={{ iconName: 'Settings' }}
                    styles={{ root: { padding: '10px 8px 10px 12px' } }}
                    text={'Project settings'}
                    onClick={() => history.push(`/orgs/${orgId}/projects/${projectId}/settings`)} />
            </Stack.Item>
        </Stack>
    );
}

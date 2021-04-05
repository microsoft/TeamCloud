// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { useHistory, useLocation, useParams } from 'react-router-dom';
import { Text, Breadcrumb, IBreadcrumbItem } from '@fluentui/react';
import { endsWithLowerCase, includesLowerCase, matchesLowerCase, matchesRouteParam } from '../Utils';
import { useOrgs, useProject, useProjectComponent } from '../hooks';

export const HeaderBreadcrumb: React.FC = () => {

    const location = useLocation();
    const history = useHistory();
    const { orgId, projectId, navId, itemId, settingId } = useParams() as { orgId: string, projectId: string, navId: string, itemId: string, settingId: string };

    const { data: orgs } = useOrgs();
    const { data: project } = useProject();
    const { data: component } = useProjectComponent();

    const _breadcrumbs = (): IBreadcrumbItem[] => {
        const crumbs: IBreadcrumbItem[] = [];

        if (orgId === undefined)
            return crumbs;

        const orgPath = `/orgs/${orgId}`;
        const orgName = orgs?.find(o => matchesRouteParam(o, orgId))?.displayName ?? orgId;
        const orgCrumb = { key: orgId, text: orgName, onClick: () => history.push(orgPath) };

        if (location.pathname.toLowerCase().endsWith('/projects/new')) {

            // Org / Projects / New Project
            crumbs.push({ key: 'projects', text: 'Projects', onClick: () => history.push(orgPath) });
            crumbs.push({ key: 'new', text: 'New Project' });

        } else if (projectId !== undefined) {

            // Org / Projects / Project
            crumbs.push({ key: 'projects', text: 'Projects', onClick: () => history.push(orgPath) });
            crumbs.push({ key: projectId, text: project?.displayName ?? projectId, onClick: () => history.push(`${orgPath}/projects/${projectId}`) });

            // Org / Projects / Project / Category
            if (navId !== undefined) {
                crumbs.push({ key: navId, text: navId, onClick: () => history.push(`${orgPath}/projects/${projectId}/${navId}`) });

                if (endsWithLowerCase(location.pathname, '/new')) {
                    crumbs.push({ key: 'new', text: `New ${navId.slice(0, -1)}` });

                } else if (itemId !== undefined) {

                    if (matchesLowerCase(navId, 'components')) {
                        crumbs.push({ key: itemId, text: component?.displayName ?? itemId, onClick: () => history.push(`${orgPath}/projects/${projectId}/components/${component?.slug}`) });
                    }
                }
            }
        }

        if (includesLowerCase(location.pathname, '/settings')) {

            // Org / Settings
            // Org / Projects / Project / Settings
            const settingPath = projectId === undefined ? `${orgPath}/settings` : `${orgPath}/projects/${projectId}/settings`;
            crumbs.push({ key: 'settings', text: 'Settings', onClick: () => history.push(settingPath) });

            // Org / Settings / Setting
            // Org / Projects / Project / Settings / Setting
            if (settingId !== undefined) {
                crumbs.push({ key: settingId, text: settingId, onClick: () => history.push(`${settingPath}/${settingId}`) });
                if (projectId === undefined && endsWithLowerCase(location.pathname, '/new'))
                    crumbs.push({ key: 'new', text: `New ${settingId.slice(0, -1)}` });
            }
        }

        // check for any crumbs before adding the org
        // so we never only show the org crumb
        if (crumbs.length > 0)
            crumbs.unshift(orgCrumb);

        return crumbs;
    };

    const bcSize = '14px';
    const bcWeight = 400;
    const bcColor = 'rgba(0,0,0,.55)';

    const _bcTextStyles: any = {
        root: { fontSize: bcSize, fontWeight: bcWeight, color: bcColor, padding: '0px 6px' }
    }

    const _bcItemStyles: any = {
        textTransform: 'capitalize', fontSize: bcSize, fontWeight: bcWeight, color: bcColor,
        selectors: { ':last-child.ms-Breadcrumb-item': { fontSize: bcSize, fontWeight: bcWeight, color: bcColor } }
    }

    const _bcItemLinkStyles: any = {
        textTransform: 'capitalize', fontSize: bcSize, fontWeight: bcWeight, color: bcColor, lineHeight: '27px', borderRadius: '2px',
        selectors: { ':last-child.ms-Breadcrumb-itemLink': { fontSize: bcSize, fontWeight: bcWeight, color: bcColor } }
    }

    return (
        <Breadcrumb
            dividerAs={(p) => <Text styles={_bcTextStyles}>/</Text>}
            items={_breadcrumbs()}
            onReduceData={() => undefined}
            styles={{
                root: { margin: 'auto' },
                listItem: { alignItems: 'center', },
                item: _bcItemStyles,
                itemLink: _bcItemLinkStyles
            }} />
    );
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Nav, INavLinkGroup, INavLink, getTheme, INavStyles } from '@fluentui/react';

export interface IRootNavProps {
    isBreadcrumbs: boolean;
    locationNameLowerCase: string;
}

export const RootNav: React.FunctionComponent<IRootNavProps> = (props) => {

    const { isBreadcrumbs, locationNameLowerCase } = props;

    const _getDisplayName = () => {
        if (locationNameLowerCase === 'projects') return 'Projects';
        if (locationNameLowerCase === 'projecttypes') return 'Project Types';
        if (locationNameLowerCase === 'providers') return 'Providers';
        return '';
    };

    const theme = getTheme();

    const _navLinkGroups = (): INavLinkGroup[] => {
        let links: INavLink[] = [];
        if (locationNameLowerCase !== 'projects')
            links.push({
                key: 'projects',
                name: 'Projects',
                url: '/'
            });
        if (locationNameLowerCase !== 'projecttypes')
            links.push({
                key: 'projectTypes',
                name: 'Project Types',
                url: '/projectTypes'
            });
        if (locationNameLowerCase !== 'providers')
            links.push({
                key: 'providers',
                name: 'Providers',
                url: '/providers'
            });
        return [{
            links: [{
                key: 'root',
                name: _getDisplayName(),
                url: isBreadcrumbs ? (locationNameLowerCase === 'projects' ? '/' : `/${locationNameLowerCase}`) : '',
                links: links
            }]
        }]
    };

    const _navStyles = (): INavStyles => ({
        // const _navStyles = {
        root: [{
            padding: '7px',
            zIndex: 9999
        }],
        group: {
            textTransform: 'capitalize',
        },
        linkText: {
            fontSize: '16px',
            lineHeight: '36px',
        },
        groupContent: {
            selectors: {
                '.ms-Nav-navItems:first-child > .ms-Nav-navItem': {
                    backgroundColor: 'transparent',
                },
                '.ms-Nav-navItems:first-child > .ms-Nav-navItem > .ms-Nav-compositeLink:hover > .ms-Nav-link': {
                    backgroundColor: 'transparent',
                },
                '.ms-Nav-navItems:first-child > .ms-Nav-navItem > .ms-Nav-compositeLink > .ms-Nav-link .ms-Nav-linkText': {
                    fontSize: '18px',
                    fontWeight: isBreadcrumbs ? '400' : '600',
                }
            }
        },
        navItem: {
            selectors: {
                '.ms-Nav-navItems:last-child': {
                    bordeRadius: theme.effects.roundedCorner2,
                    boxShadow: theme.effects.elevation16
                },
                '.ms-Nav-navItems:last-child > .ms-Nav-navItem': {
                    backgroundColor: theme.palette.white,
                }
            }
        },
        navItems: {},
        chevronIcon: {},
        chevronButton: {
            color: 'rgb(50, 49, 48)',
            backgroundColor: 'transparent',
            borderStyle: 'none',
            selectors: {
                '::after': {
                    borderStyle: 'none',
                    borderLeft: 'none'
                }
            }

        },
        compositeLink: {},
        link: {
            backgroundColor: 'transparent',
            selectors: {
                '::after': {
                    borderStyle: 'none',
                    borderLeft: 'none'
                }
            }
        },
    });

    return (
        <Nav
            isOnTop={true}
            styles={_navStyles()}
            groups={_navLinkGroups()} />
    );
}

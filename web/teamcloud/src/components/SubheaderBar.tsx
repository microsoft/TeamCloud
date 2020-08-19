// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Stack, IBreadcrumbItem, Breadcrumb, CommandBar, ICommandBarItemProps, Separator, ICommandBarStyles, INavLinkGroup, Nav, INavLink, INavStyles, getTheme } from '@fluentui/react';
import { useLocation } from 'react-router-dom';

export interface ISubheaderBarProps {
    breadcrumbs: IBreadcrumbItem[];
    commandBarItems: ICommandBarItemProps[];
    centerCommandBarItems?: ICommandBarItemProps[];
    commandBarWidth?: string;
    breadcrumbsWidth?: string;
}

export const SubheaderBar: React.FunctionComponent<ISubheaderBarProps> = (props) => {

    const locaiton = useLocation();

    const _getName = () => {
        let parts = locaiton.pathname.split('/').filter(s => s);
        return parts.length > 0 ? parts[0] : 'Projects';
    };

    const _getDisplayName = () => {
        let name = _getName();
        if (name.toLowerCase() === 'projects') return 'Projects';
        if (name.toLowerCase() === 'projecttypes') return 'Project Types';
        if (name.toLowerCase() === 'providers') return 'Providers';
        return '';
    };

    const _getMargin = () => {
        let name = _getName();
        if (name.toLowerCase() === 'projects') return '93px';
        if (name.toLowerCase() === 'projecttypes') return '104px';
        if (name.toLowerCase() === 'providers') return '95px';
    };

    // const _getUrl = () => {
    //     let name = _getName();
    //     return name.toLowerCase() === 'projects' ? '/' : `/${name}`;
    // };

    const _commandBarWidth = props.commandBarWidth ?? '181px';
    const _breadcrumbsWidth = props.breadcrumbsWidth ?? '181px';

    const _stackStyles = { root: { paddingTop: '10px' } }

    const _breadcrumbStyles = () => ({
        root: {
            minWidth: _breadcrumbsWidth,
            marginLeft: _getMargin()
        }
    });

    const _centerCommandBarStyles: ICommandBarStyles = {
        root: {
            paddingTop: '4px',
            minWidth: '200px'
        },
        primarySet: {
            alignItems: 'center'
        }
    }

    const _commandBarStyles: ICommandBarStyles = {
        root: {
            marginTop: '4px',
            minWidth: _commandBarWidth
        }
    }

    const _getCenterCommandBar = props.centerCommandBarItems ? (
        <Stack.Item>
            <CommandBar
                styles={_centerCommandBarStyles}
                items={props.centerCommandBarItems} />
        </Stack.Item>
    ) : null;


    const _navLinkGroups = (): INavLinkGroup[] => {
        let name = _getName();
        let links: INavLink[] = [];
        if (name.toLowerCase() !== 'projects')
            links.push({
                key: 'projects',
                name: 'Projects',
                url: '/'
            });
        if (name.toLowerCase() !== 'projecttypes')
            links.push({
                key: 'projectTypes',
                name: 'Project Types',
                url: '/projectTypes'
            });
        if (name.toLowerCase() !== 'providers')
            links.push({
                key: 'providers',
                name: 'Providers',
                url: '/providers'
            });
        return [{
            links: [{
                key: 'root',
                name: _getDisplayName(),
                url: '',
                links: links
            }]
        }]
    };

    const theme = getTheme();
    const _navStyles = (): INavStyles => ({
        // const _navStyles = {
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
                    fontWeight: props.breadcrumbs.length > 1 ? '400' : '600',
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
        chevronButton: {},
        compositeLink: {},
        link: {},
        root: [{
            padding: '7px',
            zIndex: 9999
        }]
    });

    return (
        <>
            <Stack
                wrap
                horizontal
                verticalFill
                styles={_stackStyles}
                horizontalAlign='space-between'
                tokens={{ padding: '0 8px 0 8px' }}>
                <Stack.Item>
                    <Stack horizontal>
                        <Nav
                            isOnTop={true}
                            styles={_navStyles()}
                            groups={_navLinkGroups()} />
                        <Breadcrumb
                            items={props.breadcrumbs}
                            styles={_breadcrumbStyles()} />
                    </Stack>
                </Stack.Item>
                {_getCenterCommandBar}
                <Stack.Item>
                    <CommandBar
                        styles={_commandBarStyles}
                        items={props.commandBarItems} />
                </Stack.Item>
            </Stack>
            <Separator />
        </>
    );
}

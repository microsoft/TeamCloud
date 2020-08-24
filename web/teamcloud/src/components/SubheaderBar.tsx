// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Stack, IBreadcrumbItem, Breadcrumb, CommandBar, ICommandBarItemProps, Separator, ICommandBarStyles } from '@fluentui/react';
import { useLocation } from 'react-router-dom';
import { RootNav } from './RootNav';

export interface ISubheaderBarProps {
    breadcrumbs: IBreadcrumbItem[];
    commandBarItems: ICommandBarItemProps[];
    centerCommandBarItems?: ICommandBarItemProps[];
    commandBarWidth?: string;
    breadcrumbsWidth?: string;
}

export const SubheaderBar: React.FunctionComponent<ISubheaderBarProps> = (props) => {

    const locaiton = useLocation();

    const _getNameLowerCase = () => {
        let parts = locaiton.pathname.split('/').filter(s => s);
        return parts.length > 0 ? parts[0].toLowerCase() : 'projects';
    };

    const _getMargin = () => {
        let name = _getNameLowerCase();
        if (name === 'projects') return '93px';
        if (name === 'projecttypes') return '104px';
        if (name === 'providers') return '95px';
    };

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
                        <RootNav
                            isBreadcrumbs={props.breadcrumbs.length > 1}
                            locationNameLowerCase={_getNameLowerCase()} />
                        <Breadcrumb
                            items={props.breadcrumbs}
                            styles={_breadcrumbStyles()} />
                    </Stack>
                </Stack.Item>
                {_getCenterCommandBar}
                <Stack.Item>
                    <CommandBar
                        styles={_commandBarStyles}
                        items={props.commandBarItems}
                        ariaLabel='Use left and right arrow keys to navigate between commands' />
                </Stack.Item>
            </Stack>
            <Separator />
        </>
    );
}

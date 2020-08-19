// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Stack, IBreadcrumbItem, Breadcrumb, CommandBar, ICommandBarItemProps, Separator, ICommandBarStyles } from '@fluentui/react';

export interface ISubheaderBarProps {
    breadcrumbs: IBreadcrumbItem[];
    commandBarItems: ICommandBarItemProps[];
    centerCommandBarItems?: ICommandBarItemProps[];
    commandBarWidth?: string;
    breadcrumbsWidth?: string;
}

export const SubheaderBar: React.FunctionComponent<ISubheaderBarProps> = (props) => {

    const _commandBarWidth = props.commandBarWidth ?? '181px';
    const _breadcrumbsWidth = props.breadcrumbsWidth ?? '181px';

    const _stackStyles = { root: { paddingTop: '10px' } }

    const _breadcrumbStyles = { root: { minWidth: _breadcrumbsWidth } }

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
                tokens={{ padding: '0 24px' }}>
                <Stack.Item>
                    <Breadcrumb
                        items={props.breadcrumbs}
                        styles={_breadcrumbStyles} />
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

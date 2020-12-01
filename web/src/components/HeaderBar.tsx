// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { ITextStyles, Stack, getTheme, IStackStyles, Link } from '@fluentui/react';
import { GraphUser } from '../model';
import { HeaderBreadcrumb, UserInfo } from '.';

export interface IHeaderBarProps {
    graphUser?: GraphUser;
}

export const HeaderBar: React.FC<IHeaderBarProps> = (props) => {

    const theme = getTheme();

    const stackStyles: IStackStyles = {
        root: {
            height: '48px',
            borderBottom: `${theme.palette.neutralLight} solid 1px`
        }
    };

    const titleStyles: ITextStyles = {
        root: {
            fontWeight: 'bold',
            paddingLeft: '12px',
            color: theme.palette.themePrimary,
            fontSize: theme.fonts.mediumPlus.fontSize
        }
    };


    return (
        <header>
            <Stack horizontal verticalFill verticalAlign='center' horizontalAlign='space-between' styles={stackStyles}>
                <Stack.Item>
                    <Stack horizontal verticalFill verticalAlign='center'>
                        <Stack.Item styles={{ root: { width: '260px' } }}>
                            <Link styles={titleStyles} href='/'>
                                TeamCloud
                            </Link>
                        </Stack.Item>
                        <Stack.Item styles={{ root: { paddingLeft: '12px' } }}>
                            <HeaderBreadcrumb />
                        </Stack.Item>
                    </Stack>
                </Stack.Item>
                <Stack.Item>
                    <UserInfo
                        // user={props.user}
                        graphUser={props.graphUser}
                    // onSignOut={onSignOut}
                    />
                </Stack.Item>
            </Stack>
        </header>
    );
}

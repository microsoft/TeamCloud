// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useState } from 'react';
import { useIsAuthenticated } from '@azure/msal-react';
import { ITextStyles, Stack, getTheme, IStackStyles, Link } from '@fluentui/react';
import { getMe } from '../../MSGraph';
import { GraphUser } from '../../model';
import { HeaderBreadcrumb, UserInfo } from '.';

export interface IHeaderBarProps { }

export const HeaderBar: React.FunctionComponent<IHeaderBarProps> = (props) => {

    const isAuthenticated = useIsAuthenticated();

    const [graphUser, setGraphUser] = useState<GraphUser>();

    useEffect(() => {
        if (isAuthenticated && graphUser === undefined) {
            // console.error('getMe');
            const _setGraphUser = async () => {
                const result = await getMe();
                setGraphUser(result);
            };
            _setGraphUser();
        }
    }, [isAuthenticated, graphUser]);

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
                        graphUser={graphUser}
                    // onSignOut={onSignOut}
                    />
                </Stack.Item>
            </Stack>
        </header>
    );
}

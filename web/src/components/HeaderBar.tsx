// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useEffect, useMemo, useState } from 'react';
import { Text, ITextStyles, Stack, getTheme, IStackStyles, Separator } from '@fluentui/react';
import { UserInfo } from '.';
// import { User } from 'teamcloud';
import { GraphUser } from '../model';
import { getMe } from '../MSGraph';

export interface IHeaderBarProps {
    // user?: User;
    graphUser?: GraphUser;
    onSignOut: () => void;
}

export const HeaderBar: React.FunctionComponent<IHeaderBarProps> = (props) => {

    const theme = getTheme();

    const stackStyles: IStackStyles = {
        root: {
            height: '48px',
            borderBottom: `${theme.palette.neutralLight} solid 1px`
        }
    };

    const titleStyles: ITextStyles = {
        root: {
            height: '48px',
            width: '260px',
            fontWeight: 'bold',
            paddingLeft: '12px',
            color: theme.palette.themePrimary,
            fontSize: theme.fonts.mediumPlus.fontSize
        }
    };

    return (
        <header>
            <Stack horizontal
                verticalFill
                horizontalAlign='space-between'
                verticalAlign='center'
                styles={stackStyles}>
                <Stack.Item>
                    <Text styles={titleStyles}>TeamCloud</Text>
                </Stack.Item>
                <Stack.Item>
                    <UserInfo
                        // user={props.user}
                        graphUser={props.graphUser}
                        onSignOut={props.onSignOut} />
                </Stack.Item>
            </Stack>
        </header>
    );
}

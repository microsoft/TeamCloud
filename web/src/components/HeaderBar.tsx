// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { UserInfo } from '.';
import { Text, ITextStyles, Stack, getTheme, IStackStyles } from '@fluentui/react';
import { GraphUser, User } from '../model';
// import { User } from 'teamcloud';

export interface IHeaderBarProps {
    user?: User;
    graphUser?: GraphUser;
    onSignOut: () => void;
}

export const HeaderBar: React.FunctionComponent<IHeaderBarProps> = (props) => {

    const theme = getTheme();

    const stackStyles: IStackStyles = {
        root: {
            minHeight: '56px',
            background: theme.palette.themePrimary,
        }
    };

    const titleStyles: ITextStyles = {
        root: {
            minHeight: '56px',
            paddingLeft: '12px',
            fontSize: theme.fonts.xxLarge.fontSize,
            color: theme.palette.white
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
                    <UserInfo user={props.user} graphUser={props.graphUser} onSignOut={props.onSignOut} />
                </Stack.Item>
            </Stack>
        </header>
    );
}

import React from 'react';
import { UserInfo } from '.';
import { Text, ITextStyles, Stack, getTheme, IStackStyles } from '@fluentui/react';
import { GraphUser } from '../MSGraph';

export interface IHeaderBarProps {
    graphUser: GraphUser | undefined;
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
                    <UserInfo graphUser={props.graphUser} onSignOut={props.onSignOut} />
                </Stack.Item>
            </Stack>
        </header>
    );
}

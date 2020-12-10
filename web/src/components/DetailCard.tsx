// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Stack, IStackStyles, getTheme, ICommandBarItemProps, CommandBar, ICommandBarStyles } from '@fluentui/react';
import { CalloutLabel } from './common';

export interface IDetailCardProps {
    title?: string;
    callout?: string | number;
    commandBarItems?: ICommandBarItemProps[];
}

export const DetailCard: React.FC<IDetailCardProps> = (props) => {

    const theme = getTheme();

    const _cardStackStyles: IStackStyles = {
        root: {
            width: '100%',
            margin: '8px',
            padding: '20px 0',
            borderRadius: theme.effects.roundedCorner4,
            boxShadow: theme.effects.elevation4,
            backgroundColor: theme.palette.white
        }
    };

    const _cardStackContentStyles: IStackStyles = {
        root: {
            padding: '0 20px',
        }
    };

    const _commandBarStyles: ICommandBarStyles = {
        root: {
            marginTop: '-4px',
            marginBottom: '4px',
            padding: '0px',
            // minWidth: '150px',
        }
    };

    const _getCammandBar = (): JSX.Element | null => props.commandBarItems ? <CommandBar
        onReduceData={() => undefined}
        styles={_commandBarStyles}
        items={props.commandBarItems}
        ariaLabel='Use left and right arrow keys to navigate between commands' />
        : null;

    return (
        <Stack verticalFill styles={_cardStackStyles}>
            <Stack styles={_cardStackContentStyles} >
                <Stack styles={{ root: { minHeight: '44px' } }} horizontal horizontalAlign='space-between'>
                    <Stack.Item>
                        <CalloutLabel large title={props.title} callout={props.callout} />
                    </Stack.Item>
                    <Stack.Item>
                        {_getCammandBar()}
                    </Stack.Item>
                </Stack>
                {/* {_getTitle()} */}
                {props.children}
            </Stack>
        </Stack>
    );
}

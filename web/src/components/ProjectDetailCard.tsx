// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { Stack, IStackStyles, ITextStyles, getTheme, FontWeights, Text, ICommandBarItemProps, CommandBar, ICommandBarStyles } from '@fluentui/react';

export interface IProjectDetailCardProps {
    title?: string;
    callout?: string;
    commandBarItems?: ICommandBarItemProps[];
}

export const ProjectDetailCard: React.FunctionComponent<IProjectDetailCardProps> = (props) => {

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
    }

    const _cardStackContentStyles: IStackStyles = {
        root: {
            padding: '0 20px',
        }
    }

    const _titleStyles: ITextStyles = {
        root: {
            fontSize: '21px',
            fontWeight: FontWeights.semibold,
            marginBottom: '12px'
        }
    }

    const _calloutStyles: ITextStyles = {
        root: {
            fontSize: '13px',
            fontWeight: FontWeights.regular,
            color: 'rgb(102, 102, 102)',
            backgroundColor: theme.palette.neutralLighter,
            marginBottom: '14px',
            marginTop: '5px',
            padding: '2px 12px',
            borderRadius: '14px',
        }
    }

    const _commandBarStyles: ICommandBarStyles = {
        root: {
            marginTop: '-4px',
            marginBottom: '4px',
            padding: '0px',
            // minWidth: '150px',
        }
    }

    const _getCallout = (): JSX.Element | null => props.callout ? <Text styles={_calloutStyles}>{props.callout}</Text> : null;

    const _getTitle = (): JSX.Element | null => props.title ? <Text styles={_titleStyles}>{props.title}</Text> : null;

    const _getCammandBar = (): JSX.Element | null => props.commandBarItems ? <CommandBar
        styles={_commandBarStyles}
        items={props.commandBarItems}
        ariaLabel='Use left and right arrow keys to navigate between commands' />
        : null;

    return (
        <Stack verticalFill styles={_cardStackStyles}>
            <Stack styles={_cardStackContentStyles} >
                <Stack styles={{ root: { minHeight: '44px' } }} horizontal horizontalAlign='space-between'>
                    <Stack.Item>
                        <Stack horizontal tokens={{ childrenGap: '5px' }}>
                            {_getTitle()}
                            {_getCallout()}
                        </Stack>
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

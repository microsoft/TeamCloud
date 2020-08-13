import React from 'react';
import { Stack, IStackStyles, ITextStyles, getTheme, FontWeights, Text } from '@fluentui/react';

export interface IProjectDetailCardProps {
    title?: string;
}

export const ProjectDetailCard: React.FunctionComponent<IProjectDetailCardProps> = (props) => {

    const theme = getTheme();

    const _cardStackStyles: IStackStyles = {
        root: {
            width: '100%',
            margin: '8px',
            padding: '20px 0',
            bordeRadius: theme.effects.roundedCorner2,
            boxShadow: theme.effects.elevation4
        }
    }

    const _cardStackContentStyles: IStackStyles = {
        root: {
            padding: '0 20px',
        }
    }

    const _headingStyles: ITextStyles = {
        root: {
            fontSize: theme.fonts.large.fontSize,
            fontWeight: FontWeights.semibold,
            marginBottom: '12px'
        }
    }

    const _getTitle = (): JSX.Element | null => props.title ? <Text styles={_headingStyles}>{props.title}</Text> : null;

    return (
        <Stack verticalFill styles={_cardStackStyles}>
            <Stack styles={_cardStackContentStyles} >
                {_getTitle()}
                {props.children}
            </Stack>
        </Stack>
    );
}

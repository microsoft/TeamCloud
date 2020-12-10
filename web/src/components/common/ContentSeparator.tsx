// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { getTheme, Separator } from '@fluentui/react';

export interface IContentSeparatorProps {
    color?: any
}

export const ContentSeparator: React.FunctionComponent<IContentSeparatorProps> = (props) => {
    const theme = getTheme();
    return (<Separator styles={{ root: { selectors: { '::before': { backgroundColor: props.color ?? theme.palette.neutralQuaternary } } } }} />);;
}

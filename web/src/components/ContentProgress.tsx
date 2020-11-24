// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from 'react';
import { ProgressIndicator } from '@fluentui/react';

export interface IContentProgressProps {
    progressHidden?: boolean;
    percentComplete?: number;
}

export const ContentProgress: React.FunctionComponent<IContentProgressProps> = (props) => {

    return (
        <ProgressIndicator
            percentComplete={props.percentComplete}
            progressHidden={props.progressHidden}
            styles={{ itemProgress: { padding: '0px', marginTop: '-2px' } }} />
    );
}

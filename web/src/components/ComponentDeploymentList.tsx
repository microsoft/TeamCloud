// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { getTheme, Stack } from '@fluentui/react';
import React, { useState, useEffect } from 'react';

export interface IComponentDeploymentListProps {

}

export const ComponentDeploymentList: React.FunctionComponent<IComponentDeploymentListProps> = (props) => {

    const theme = getTheme();

    return (
        <Stack horizontal tokens={{ childrenGap: '40px' }} styles={{ root: { padding: '24px 8px' } }}>
            <Stack.Item grow styles={{
                root: { minWidth: '40%', }
            }}>
                <Stack
                    // styles={{ root: { paddingTop: '20px' } }}
                    tokens={{ childrenGap: '20px' }}>
                    <Stack.Item>

                    </Stack.Item>
                </Stack>
            </Stack.Item>
            <Stack.Item grow styles={{
                root: {
                    minWidth: '40%',
                    padding: '10px 40px',
                    borderRadius: theme.effects.roundedCorner4,
                    boxShadow: theme.effects.elevation4,
                    backgroundColor: theme.palette.white
                }
            }}>



            </Stack.Item>
        </Stack>
    );
}

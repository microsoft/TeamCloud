// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React from "react";
import { Stack } from "@fluentui/react";
import { FieldTemplateProps } from "@rjsf/core";


export const TCFieldTemplate: React.FC<FieldTemplateProps> = (props) => {
    return props.id === 'root' ? (
        <Stack styles={{ root: { minWidth: '460px' } }} tokens={{ childrenGap: '14px' }}>
            {props.children}
        </Stack>
    ) : (
        <Stack.Item grow styles={{ root: { paddingBottom: '20px' } }}>
            {props.children}
            {props.rawDescription && (<span className="ms-TextField-description" style={{ fontSize: '10px', color: 'rgb(96, 94, 92)' }}>{props.rawDescription}</span>)}
        </Stack.Item>
    );
}

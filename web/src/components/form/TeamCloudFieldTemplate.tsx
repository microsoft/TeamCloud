// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Stack } from "@fluentui/react";
import { FieldTemplateProps } from "@rjsf/core";
import React from "react";

import "./TeamCloudFieldTemplate.css"

export const TeamCloudFieldTemplate: React.FC<FieldTemplateProps> = (props) => {
	return props.id === 'root' ? (
        <Stack className={`teamCloudFieldTemplateRoot ${ (props.schema.anyOf || props.schema.oneOf) ? 'teamCloudFieldTemplateSelect' : ''}`} styles={{ root: { minWidth: '460px' } }} tokens={{ childrenGap: '14px' }}>
            {props.children}
        </Stack>
    ) : (
        <Stack.Item grow className="teamCloudFieldTemplateItem" styles={{ root: { paddingBottom: '16px' } }}>
            {props.children}
        </Stack.Item>
    );
}
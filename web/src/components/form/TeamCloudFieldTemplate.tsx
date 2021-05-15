import { Stack } from "@fluentui/react";
import { FieldTemplateProps } from "@rjsf/core";
import React from "react";

export const TeamCloudFieldTemplate: React.FC<FieldTemplateProps> = (props) => {
    // console.log(props);
	return props.id === 'root' ? (
        <Stack styles={{ root: { minWidth: '460px' } }} tokens={{ childrenGap: '14px' }}>
            {props.children}
        </Stack>
    ) : (
        <Stack.Item grow styles={{ root: { paddingBottom: '16px' } }}>
            {props.children}
        </Stack.Item>
    );
}
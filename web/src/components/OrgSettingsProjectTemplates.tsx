// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Text } from '@fluentui/react';
import { Organization } from 'teamcloud';


export interface IOrgSettingsProjectTemplatesProps {
    org?: Organization
}

export const OrgSettingsProjectTemplates: React.FunctionComponent<IOrgSettingsProjectTemplatesProps> = (props) => {


    return (
        <Text>{'Project Templates: ' + props.org?.displayName}</Text>
    );
}

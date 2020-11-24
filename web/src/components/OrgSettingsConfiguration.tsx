// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Text } from '@fluentui/react';
import { Organization } from 'teamcloud';


export interface IOrgSettingsConfigurationProps {
    org?: Organization
}

export const OrgSettingsConfiguration: React.FunctionComponent<IOrgSettingsConfigurationProps> = (props) => {


    return (
        <Text>{'Configuration: ' + props.org?.displayName}</Text>
    );
}

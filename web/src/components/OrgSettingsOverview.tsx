// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Text } from '@fluentui/react';
import { Organization } from 'teamcloud';


export interface IOrgSettingsOverviewProps {
    org?: Organization
}

export const OrgSettingsOverview: React.FunctionComponent<IOrgSettingsOverviewProps> = (props) => {


    return (
        <Text>{'Overview: ' + props.org?.displayName}</Text>
    );
}

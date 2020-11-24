// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Text } from '@fluentui/react';
import { Organization } from 'teamcloud';


export interface IOrgSettingsMembersProps {
    org?: Organization
}

export const OrgSettingsMembers: React.FunctionComponent<IOrgSettingsMembersProps> = (props) => {


    return (
        <Text>{'Members: ' + props.org?.displayName}</Text>
    );
}

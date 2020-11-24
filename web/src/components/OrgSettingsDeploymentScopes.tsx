// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import React, { useState, useEffect } from 'react';
import { Text } from '@fluentui/react';
import { Organization } from 'teamcloud';

export interface IOrgSettingsDeploymentScopesProps {
    org?: Organization
}

export const OrgSettingsDeploymentScopes: React.FunctionComponent<IOrgSettingsDeploymentScopesProps> = (props) => {


    return (
        <Text>{'Deployment Scopes: ' + props.org?.displayName}</Text>
    );
}

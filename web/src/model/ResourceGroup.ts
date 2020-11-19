// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Tags } from ".";

export interface ResourceGroups {
    value: ResourceGroup[];
    nextLink?: string;
}

export interface ResourceGroup {
    id: string;
    location: string;
    managedBy: string;
    name: string;
    properties: ResourceGroupProperties;
    tags: Tags;
    type: string;
}

export interface ResourceGroupProperties {
    provisioningState: string;
}


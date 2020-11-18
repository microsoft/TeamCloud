// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export interface ManagementGroups {
    value: ManagementGroup[];
}

export interface ManagementGroupResource {
    id: string;
    type: string;
    name: string;
    displayName?: string;
}

export interface ManagementGroup extends ManagementGroupResource {
    properties: ManagementGroupProperties;
}

export interface ManagementGroupProperties {
    tenantId: string;
    displayName: string;
    details: ManagementGroupDetails;
    children?: ManagementGroupResource[];
}

export interface ManagementGroupDetails {
    version: number;
    updatedTime: Date;
    updatedBy: string;
    parent: ManagementGroupResource
}

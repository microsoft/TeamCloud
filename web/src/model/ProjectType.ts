// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { IIdentifiable, ITags, IProperties } from '.'

export interface ProjectType extends IIdentifiable, ITags, IProperties {
    isDefault?: boolean;
    region: string;
    subscriptions: string[];
    subscriptionCapacity: number;
    resourceGroupNamePrefix?: string;
    providers: ProviderReference[];
}

export interface ProviderReference extends IIdentifiable, IProperties {
    dependsOn?: string[];
    metadata?: Map<string, Map<string, string>>;
}

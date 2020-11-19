// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { Tags } from ".";

export interface Subscriptions {
    value: Subscription[];
    count: Count;
}

export interface Count {
    type: string;
    value: number;
}

export interface Subscription {
    id: string;
    authorizationSource: string;
    managedByTenants: ManagedByTenant[];
    tags: Tags;
    subscriptionId: string;
    tenantId: string;
    displayName: string;
    state: string;
    subscriptionPolicies: SubscriptionPolicies;
}

export interface ManagedByTenant {
    tenantId: string;
}

export interface SubscriptionPolicies {
    locationPlacementId: string;
    quotaID: string;
    spendingLimit: string;
}

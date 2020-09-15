// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { IIdentifiable } from '.';

export interface AzureResourceGroup extends IIdentifiable {
    name: string;
    subscriptionId: string;
    region: string;
}

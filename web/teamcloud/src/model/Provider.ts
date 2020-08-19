// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { IIdentifiable, IProperties, AzureResourceGroup } from "./index";

export interface Provider extends IIdentifiable, IProperties {
    id: string;
    url: string;
    // authCode: string;
    principalId: string;
    version: string;
    resourceGroup: AzureResourceGroup;
    events?: string[];
    properties?: Record<string, string>;
    registered?: string;
    commandMode: ProviderCommandMode;
}


export enum ProviderCommandMode {
    Simple = 'Simple',
    Extended = 'Extended'
}

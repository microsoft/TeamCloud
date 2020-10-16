// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { IIdentifiable, IProperties, AzureResourceGroup } from '.';

export interface Provider extends IIdentifiable, IProperties {
    id: string;
    url: string;
    // authCode: string;
    principalId: string;
    version: string;
    resourceGroup: AzureResourceGroup;
    events?: string[];
    registered?: string;
    commandMode: ProviderCommandMode;
}


export enum ProviderCommandMode {
    Simple = 'Simple',
    Extended = 'Extended'
}

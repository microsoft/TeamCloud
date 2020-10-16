// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { IIdentifiable } from '.'

export interface Component extends IIdentifiable {
    offerId: string;
    projectId: string;
    providerId: string;
    requestedBy: string;
    displayName: string;
    description: string;
    inputJson: string;
    valueJson: string;
}

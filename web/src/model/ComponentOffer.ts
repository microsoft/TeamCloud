// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { IIdentifiable } from '.'

export interface ComponentOffer extends IIdentifiable {
    providerId: string;
    displayName: string;
    description: string;
    inputJsonSchema: string;
}

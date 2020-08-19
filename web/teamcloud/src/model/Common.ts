// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export interface IIdentifiable {
    id: string;
}

export interface IProperties {
    properties?: Record<string, string>;
}

export interface ITags {
    tags?: Record<string, string>;
}

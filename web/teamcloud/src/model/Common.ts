// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ReferenceLink } from '.'

export interface IIdentifiable {
    id: string;
}

export interface Properties {
    [key: string]: string
}

export interface ReferenceLinks {
    [key: string]: ReferenceLink
}

export interface IProperties {
    // properties?: Map<string, string>;
    // properties?: Record<string, string>;
    // properties?: Property[];
    properties?: Properties;
    // properties?: { key: string, value: string }[];
}

export interface ITags {
    tags?: Map<string, string>;
}

export interface ILinks {
    _links: ReferenceLinks;
}

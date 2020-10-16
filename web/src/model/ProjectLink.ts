// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

export interface ProjectLink {
    id: string;
    href: string;
    title: string;
    type: ProjectLinkType;
}

export enum ProjectLinkType {
    Link = 'Link',
    Readme = 'Readme',
    Service = 'Service',
    AzureResource = 'AzureResource',
    GitRepository = 'GitRepository'
}

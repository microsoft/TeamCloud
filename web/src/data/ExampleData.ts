// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ProjectLink, ProjectLinkType } from '../model'

export const ExampleProjectLinks: ProjectLink[] = [
    {
        id: '00000000-0000-0000-0000-00000000000a',
        title: 'GitHub Team',
        href: 'https://github.com/orgs/TeamCloudMSFT/teams',
        type: ProjectLinkType.Link
    },
    {
        id: '00000000-0000-0000-0000-00000000000b',
        title: 'GitHub Repo',
        href: 'https://github.com/TeamCloudMSFT',
        type: ProjectLinkType.GitRepository
    },
    {
        id: '00000000-0000-0000-0000-00000000000c',
        title: 'Add a Repository to this Project',
        href: 'https://github.com/TeamCloudMSFT',
        type: ProjectLinkType.Readme
    },
    {
        id: '00000000-0000-0000-0000-00000000000d',
        title: 'DevTestLabs Portal',
        href: 'https://github.com/TeamCloudMSFT',
        type: ProjectLinkType.AzureResource
    },
    {
        id: '00000000-0000-0000-0000-00000000000e',
        title: 'DevTestLabs Jenkins Service',
        href: 'https://github.com/TeamCloudMSFT',
        type: ProjectLinkType.Service
    },
    {
        id: '00000000-0000-0000-0000-00000000000f',
        title: 'DevOps Repo',
        href: 'https://github.com/TeamCloudMSFT',
        type: ProjectLinkType.GitRepository
    },
    {
        id: '00000000-0000-0000-0000-00000000000g',
        title: 'DevOps Project Boards',
        href: 'https://github.com/TeamCloudMSFT',
        type: ProjectLinkType.Link
    },
    {
        id: '00000000-0000-0000-0000-00000000000h',
        title: 'Application Insights Portal',
        href: 'https://github.com/TeamCloudMSFT',
        type: ProjectLinkType.AzureResource
    },
    {
        id: '00000000-0000-0000-0000-00000000000i',
        title: 'Application Insights Alerts',
        href: 'https://github.com/TeamCloudMSFT',
        type: ProjectLinkType.Link
    }
]

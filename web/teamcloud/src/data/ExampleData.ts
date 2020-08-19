// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ProjectLink, ProjectLinkType } from '../model'

export const ExampleProjectLinks: ProjectLink[] = [
    {
        projectId: '',
        providerId: 'github',
        id: '00000000-0000-0000-0000-00000000000a',
        name: 'GitHub Team',
        value: 'https://github.com/orgs/TeamCloudMSFT/teams',
        location: '',
        isSecret: false,
        isShared: false,
        dataType: ProjectLinkType.Link
    },
    {
        projectId: '',
        providerId: 'github',
        id: '00000000-0000-0000-0000-00000000000b',
        name: 'GitHub Repo',
        value: 'https://github.com/TeamCloudMSFT',
        location: '',
        isSecret: false,
        isShared: false,
        dataType: ProjectLinkType.Repository
    },
    {
        projectId: '',
        providerId: 'github',
        id: '00000000-0000-0000-0000-00000000000c',
        name: 'Add a Repository to this Project',
        value: 'https://github.com/TeamCloudMSFT',
        location: '',
        isSecret: false,
        isShared: false,
        dataType: ProjectLinkType.Readme
    },
    {
        projectId: '',
        providerId: 'azure.devtestlabs',
        id: '00000000-0000-0000-0000-00000000000d',
        name: 'DevTestLabs Portal',
        value: 'https://github.com/TeamCloudMSFT',
        location: '',
        isSecret: false,
        isShared: false,
        dataType: ProjectLinkType.Resource
    },
    {
        projectId: '',
        providerId: 'azure.devtestlabs',
        id: '00000000-0000-0000-0000-00000000000e',
        name: 'DevTestLabs Jenkins Service',
        value: 'https://github.com/TeamCloudMSFT',
        location: '',
        isSecret: false,
        isShared: false,
        dataType: ProjectLinkType.Service
    },
    {
        projectId: '',
        providerId: 'azure.devops',
        id: '00000000-0000-0000-0000-00000000000f',
        name: 'DevOps Repo',
        value: 'https://github.com/TeamCloudMSFT',
        location: '',
        isSecret: false,
        isShared: false,
        dataType: ProjectLinkType.Repository
    },
    {
        projectId: '',
        providerId: 'azure.devops',
        id: '00000000-0000-0000-0000-00000000000g',
        name: 'DevOps Project Boards',
        value: 'https://github.com/TeamCloudMSFT',
        location: '',
        isSecret: false,
        isShared: false,
        dataType: ProjectLinkType.Link
    },
    {
        projectId: '',
        providerId: 'azure.appinsights',
        id: '00000000-0000-0000-0000-00000000000h',
        name: 'Application Insights Portal',
        value: 'https://github.com/TeamCloudMSFT',
        location: '',
        isSecret: false,
        isShared: false,
        dataType: ProjectLinkType.Resource
    },
    {
        projectId: '',
        providerId: 'azure.appinsights',
        id: '00000000-0000-0000-0000-00000000000i',
        name: 'Application Insights Alerts',
        value: 'https://github.com/TeamCloudMSFT',
        location: '',
        isSecret: false,
        isShared: false,
        dataType: ProjectLinkType.Link
    }
]

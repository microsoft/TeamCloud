// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { IIdentifiable, ITags, IProperties, ILinks, AzureResourceGroup, ProjectType, User } from '.'

export interface Project extends IIdentifiable, ITags, IProperties, ILinks {
    name: string;
    type: ProjectType;
    resourceGroup: AzureResourceGroup;
    users: User[];
}

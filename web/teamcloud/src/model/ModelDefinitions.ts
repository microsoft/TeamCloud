// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ProjectUserRole, ITags, IProperties } from './index'

export interface ProjectDefinition extends ITags, IProperties {
    name: string
    projectType: string
    users: UserDefinition[];
}

export interface UserDefinition extends IProperties {
    identifier: string;
    role: ProjectUserRole;
}

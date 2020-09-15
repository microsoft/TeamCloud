// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { UserDefinition, ITags, IProperties } from '.'

export interface ProjectDefinition extends ITags, IProperties {
    name: string
    projectType: string
    users: UserDefinition[];
}

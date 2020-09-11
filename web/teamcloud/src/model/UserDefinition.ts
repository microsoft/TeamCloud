// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ProjectUserRole, IProperties } from '.'

export interface UserDefinition extends IProperties {
    identifier: string;
    role: ProjectUserRole;
}

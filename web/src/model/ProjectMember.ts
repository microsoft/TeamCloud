// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ProjectMembership } from 'teamcloud';
import { Member } from '.';

export interface ProjectMember extends Member {
    projectMembership: ProjectMembership;
}

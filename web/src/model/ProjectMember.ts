// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { GraphUser, Member } from '.';
import { User, ProjectMembership } from 'teamcloud';

export interface ProjectMember extends Member {
    projectMembership: ProjectMembership;
}

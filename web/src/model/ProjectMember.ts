// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { GraphUser } from '.';
import { User, ProjectMembership } from 'teamcloud';

export interface ProjectMember {
    user: User;
    graphUser?: GraphUser;
    projectMembership: ProjectMembership;
}

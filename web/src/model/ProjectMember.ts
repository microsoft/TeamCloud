// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { User, ProjectMembership, GraphUser } from '.';

export interface ProjectMember {
    user: User;
    graphUser?: GraphUser;
    projectMembership: ProjectMembership;
}

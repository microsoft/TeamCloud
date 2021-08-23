// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { ProjectMembership, User } from 'teamcloud';
import { GraphPrincipal, Member } from '.';

export class ProjectMember extends Member {

    projectMembership: ProjectMembership;

    isProjectOwner(this: ProjectMember) {
        const role = this.projectMembership.role.toLowerCase();
        return this.isOrgAdmin() || role === 'owner';
    }

    isProjectAdmin(this: ProjectMember) {
        const role = this.projectMembership.role.toLowerCase();
        return this.isProjectOwner() || role === 'admin'
    }

    constructor(user: User, graphPrincipal: GraphPrincipal, projectId: string) {
        super(user, graphPrincipal);
        this.projectMembership = this.user.projectMemberships!.find(m => m.projectId === projectId)!
    }
}

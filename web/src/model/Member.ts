// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { User } from 'teamcloud';
import { GraphPrincipal } from '.';

export class Member {

    user: User;
    graphPrincipal?: GraphPrincipal;

    isOrgOwner(this: Member) {
        const role = this.user.role.toLowerCase();
        return role === 'owner';
    }

    isOrgAdmin(this: Member) {
        const role = this.user.role.toLowerCase();
        return this.isOrgOwner() || role === 'admin'
    }

    constructor(user: User, graphPrincipal: GraphPrincipal) {
        this.user = user;
        this.graphPrincipal = graphPrincipal;
    }
}

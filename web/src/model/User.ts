// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import { IIdentifiable, IProperties } from '.'

export interface User extends IIdentifiable, IProperties {
    userType: UserType;
    role: TeamCloudUserRole;
    projectMemberships?: ProjectMembership[];
}

export interface ProjectMembership extends IProperties {
    projectId: string;
    role: ProjectUserRole;
}

export enum TeamCloudUserRole {
    None = 'None',
    Provider = 'Provider',
    Creator = 'Creator',
    Admin = 'Admin'
}

export enum ProjectUserRole {
    None = 'None',
    Provider = 'Provider',
    Member = 'Member',
    Owner = 'Owner'
}

export enum UserType {
    User = 'User',
    System = 'System',
    Provider = 'Provider',
    Application = 'Application'
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Auth;

public static class UserExtensions
{
    public static string AuthPolicy(this User user)
        => user?.Role.AuthPolicy() ?? throw new ArgumentNullException(nameof(user));

    public static string AuthPolicy(this User user, string projectId)
        => user?.RoleFor(projectId).AuthPolicy() ?? throw new ArgumentNullException(nameof(user));
}

public static class UserRoleExtensions
{
    public static string AuthPolicy(this OrganizationUserRole role)
        => $"Organization{role}";

    public static string AuthPolicy(this ProjectUserRole role)
        => $"Project{role}";
}

public static class UserRolePolicies
{
    public static string UserReadPolicy
        => $"User_Read";

    public static string UserWritePolicy
        => $"User_ReadWrite";

    public static string ComponentWritePolicy
        => $"Component_ReadWrite";

    public static string ScheduleWritePolicy
        => $"Schedule_ReadWrite";
}

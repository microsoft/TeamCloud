/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data.Core;

namespace TeamCloud.API.Auth
{
    public static class UserRoleExtensions
    {
        public static string PolicyRoleName(this TeamCloudUserRole role)
            => $"TeamCloud_{role}";

        public static string PolicyRoleName(this ProjectUserRole role)
            => $"Project_{role}";
    }

    public static class UserRolePolicies
    {
        public static string UserReadPolicy
            => $"User_Read";

        public static string UserWritePolicy
            => $"User_ReadWrite";

        public static string ProviderReadPolicyRoleName
            => $"Provider_Read";

        public static string ProviderWritePolicyRoleName
            => $"Provider_ReadWrite";
    }

    public static class ProviderUserRoles
    {
        public static string ProviderReadPolicyRoleName
            => $"Provider_Read";

        public static string ProviderWritePolicyRoleName
            => $"Provider_ReadWrite";
    }
}

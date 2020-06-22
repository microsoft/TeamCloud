/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Internal.Data
{
    public static class UserRoleExtensions
    {
        public static string PolicyRoleName(this TeamCloudUserRole role)
            => $"TeamCloud_{role}";

        public static string PolicyRoleName(this ProjectUserRole role)
            => $"Project_{role}";
    }
}

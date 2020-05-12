/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TeamCloud.Model.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TeamCloudUserRole
    {
        None, Creator, Admin
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProjectUserRole
    {
        None, Member, Owner
    }

    public static class UserRoleExtensions
    {
        public static string PolicyRoleName(this TeamCloudUserRole role)
            => $"TeamCloud_{role}";

        public static string PolicyRoleName(this ProjectUserRole role)
            => $"Project_{role}";
    }
}

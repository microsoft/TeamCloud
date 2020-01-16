/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Model.Data
{
    public static class UserRoles
    {
        public static class TeamCloud
        {
            public const string Creator = nameof(Creator);

            public const string Admin = nameof(Admin);
        }

        public static class Project
        {
            public const string Member = nameof(Member);

            public const string Owner = nameof(Owner);
        }
    }
}

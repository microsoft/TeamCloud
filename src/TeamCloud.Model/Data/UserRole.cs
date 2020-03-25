/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Diagnostics.CodeAnalysis;

namespace TeamCloud.Model.Data
{
    public static class UserRoles
    {
        [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Provides constants only")]
        public static class TeamCloud
        {
            public const string Creator = nameof(Creator);

            public const string Admin = nameof(Admin);
        }

        [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Provides constants only")]
        public static class Project
        {
            public const string Member = nameof(Member);

            public const string Owner = nameof(Owner);
        }
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.API.Auth
{
    public static class AuthPolicies
    {
        public const string Default = "default";

        public const string OrganizationOwner = nameof(OrganizationOwner);
        public const string OrganizationAdmin = nameof(OrganizationAdmin);
        public const string OrganizationMember = nameof(OrganizationMember);
        public const string OrganizationRead = nameof(OrganizationRead);

        public const string ProjectOwner = nameof(ProjectOwner);
        public const string ProjectAdmin = nameof(ProjectAdmin);
        public const string ProjectMember = nameof(ProjectMember);

        public const string OrganizationUserWrite = nameof(OrganizationUserWrite);
        public const string ProjectUserWrite = nameof(ProjectUserWrite);

        public const string ProjectComponentOwner = nameof(ProjectComponentOwner);
    }
}

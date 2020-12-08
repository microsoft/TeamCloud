/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.API.Auth
{
    public static class AuthPolicies
    {
        public const string Default = "default";

        public const string Owner = "owner";

        public const string Admin = "admin";

        public const string OrganizationRead = "organizationRead";

        public const string OrganizationDelete = "organizationDelete";

        public const string UserRead = "userRead";
        public const string UserWrite = "userWrite";

        public const string ProjectUserWrite = "projectUserWrite";

        public const string ProjectLinkWrite = "projectLinkWrite";

        public const string ProjectRead = "projectRead";
        public const string ProjectWrite = "projectWrite";
        public const string ProjectCreate = "projectCreate";
        public const string ProjectDelete = "projectDelete";

        public const string ProjectIdentityRead = "projectIdentityRead";

        public const string ProviderDataRead = "providerDataRead";
        public const string ProviderDataWrite = "providerDataWrite";

        public const string ProviderOfferRead = "ProviderOfferRead";

        public const string ProviderOfferWrite = "providerOfferWrite";
        public const string ProviderComponentWrite = "providerComponentWrite";

        public const string ProjectComponentRead = "projectComponentRead";
        public const string ProjectComponentWrite = "projectComponentWrite";

        public const string ProjectComponentUpdate = "ProjectComponentUpdate";
    }
}

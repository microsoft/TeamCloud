/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.API.Auth
{
    public static class AuthPolicies
    {
        public const string Default = "default";
        public const string Admin = "admin";
        public const string UserRead = "userRead";
        public const string UserWrite = "userWrite";
        // public const string ProjectUserRead = "projectUserRead";
        public const string ProjectUserWrite = "projectUserWrite";
        public const string ProjectRead = "projectRead";
        public const string ProjectWrite = "projectWrite";
        public const string ProjectCreate = "projectCreate";
        // public const string ProjectDelete = "projectDelete";
        public const string ProjectIdentityRead = "projectIdentityRead";
        public const string ProviderDataRead = "providerDataRead";
        public const string ProviderDataWrite = "providerDataWrite";
    }
}

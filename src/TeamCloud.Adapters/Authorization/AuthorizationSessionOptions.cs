/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Adapters.Authorization
{
    public sealed class AuthorizationSessionOptions : IAuthorizationSessionOptions
    {
        public static IAuthorizationSessionOptions Default
            => new AuthorizationSessionOptions();

        private AuthorizationSessionOptions()
        { }

        public string ConnectionString
            => Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    }
}

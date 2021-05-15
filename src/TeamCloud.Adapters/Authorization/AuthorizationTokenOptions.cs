/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Adapters.Authorization
{
    public sealed class AuthorizationTokenOptions : IAuthorizationTokenOptions
    {
        public static IAuthorizationTokenOptions Default
            => new AuthorizationTokenOptions();

        private AuthorizationTokenOptions()
        { }

        public string ConnectionString
            => Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    }
}

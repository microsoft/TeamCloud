/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.Adapters.Authorization
{
    [Options]
    public sealed class AuthorizationTokenOptions : IAuthorizationTokenOptions
    {
        private readonly AzureStorageOptions azureStorageOptions;

        public AuthorizationTokenOptions(AzureStorageOptions azureStorageOptions)
        {
            this.azureStorageOptions = azureStorageOptions ?? throw new ArgumentNullException(nameof(azureStorageOptions));
        }

        public string ConnectionString
            => azureStorageOptions.ConnectionString ?? Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.Orchestrator.Options;

[Options]
public sealed class TeamCloudAdapterAuthorizeOptions : IAuthorizationSessionOptions, IAuthorizationTokenOptions
{
    private readonly AzureStorageOptions azureStorageOptions;

    public TeamCloudAdapterAuthorizeOptions(AzureStorageOptions azureStorageOptions)
    {
        this.azureStorageOptions = azureStorageOptions ?? throw new ArgumentNullException(nameof(azureStorageOptions));
    }

    string IAuthorizationSessionOptions.ConnectionString => azureStorageOptions.ConnectionString;

    string IAuthorizationTokenOptions.ConnectionString => azureStorageOptions.ConnectionString;
}

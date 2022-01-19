/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Azure;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.API.Options;

[Options]
public sealed class TeamCloudAzureSessionOptions : IAzureSessionOptions
{
    private readonly AzureResourceManagerOptions azureRMOptions;

    public TeamCloudAzureSessionOptions(AzureResourceManagerOptions azureRMOptions)
    {
        this.azureRMOptions = azureRMOptions ?? throw new System.ArgumentNullException(nameof(azureRMOptions));
    }

    string IAzureSessionOptions.TenantId => azureRMOptions.TenantId;

    string IAzureSessionOptions.ClientId => azureRMOptions.ClientId;

    string IAzureSessionOptions.ClientSecret => azureRMOptions.ClientSecret;
}

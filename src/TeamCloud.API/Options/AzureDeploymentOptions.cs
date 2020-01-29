/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Azure.Deployments;
using TeamCloud.Azure.Deployments.Providers;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.API.Options
{
    [Options]
    public class AzureDeploymentOptions : IAzureDeploymentOptions, IAzureStorageArtifactsOptions
    {
        private readonly AzureResourceManagerOptions azureResourceManagerOptions;
        private readonly AzureDeploymentStorageOptions azureDeploymentStorageOptions;

        public AzureDeploymentOptions(AzureResourceManagerOptions azureResourceManagerOptions, AzureDeploymentStorageOptions azureDeploymentStorageOptions)
        {
            this.azureResourceManagerOptions = azureResourceManagerOptions ?? throw new ArgumentNullException(nameof(azureResourceManagerOptions));
            this.azureDeploymentStorageOptions = azureDeploymentStorageOptions ?? throw new ArgumentNullException(nameof(azureDeploymentStorageOptions));
        }

        string IAzureStorageArtifactsOptions.BaseUrlOverride => azureDeploymentStorageOptions.BaseUrlOverride;

        string IAzureStorageArtifactsOptions.ConnectionString => azureDeploymentStorageOptions.ConnectionString;

        string IAzureDeploymentOptions.Region => azureResourceManagerOptions.Region;
    }
}

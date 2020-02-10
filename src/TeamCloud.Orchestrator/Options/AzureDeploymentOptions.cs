/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Deployment.Providers;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.Orchestrator.Options
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

        public string Region => string.IsNullOrEmpty(azureResourceManagerOptions.Region)
            ? Environment.GetEnvironmentVariable("REGION_NAME")
            : azureResourceManagerOptions.Region;

        public string BaseUrlOverride => azureDeploymentStorageOptions.BaseUrlOverride;

        string IAzureStorageArtifactsOptions.ConnectionString => azureDeploymentStorageOptions.ConnectionString;
    }
}

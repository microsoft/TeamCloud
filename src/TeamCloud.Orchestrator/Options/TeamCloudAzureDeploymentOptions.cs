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
    public sealed class TeamCloudAzureDeploymentOptions : IAzureDeploymentOptions, IAzureStorageArtifactsOptions
    {
        private readonly AzureDeploymentOptions azureDeploymentOptions;
        private readonly AzureDeploymentStorageOptions azureDeploymentStorageOptions;

        public TeamCloudAzureDeploymentOptions(AzureDeploymentOptions azureDeploymentOptions, AzureDeploymentStorageOptions azureDeploymentStorageOptions)
        {
            this.azureDeploymentOptions = azureDeploymentOptions ?? throw new ArgumentNullException(nameof(azureDeploymentOptions));
            this.azureDeploymentStorageOptions = azureDeploymentStorageOptions ?? throw new ArgumentNullException(nameof(azureDeploymentStorageOptions));
        }

        string IAzureDeploymentOptions.DefaultLocation => azureDeploymentOptions.DefaultLocation;

        string IAzureStorageArtifactsOptions.BaseUrlOverride => azureDeploymentStorageOptions.BaseUrlOverride;

        string IAzureStorageArtifactsOptions.ConnectionString => azureDeploymentStorageOptions.ConnectionString;
    }
}

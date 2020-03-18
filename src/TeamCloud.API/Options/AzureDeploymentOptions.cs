/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Azure.Deployment;
using TeamCloud.Azure.Deployment.Providers;
using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;

namespace TeamCloud.API.Options
{
    [Options]
    public sealed class AzureDeploymentOptions : IAzureDeploymentOptions, IAzureStorageArtifactsOptions
    {
        private readonly AzureDeploymentStorageOptions azureDeploymentStorageOptions;

        public AzureDeploymentOptions(AzureDeploymentStorageOptions azureDeploymentStorageOptions)
        {
            this.azureDeploymentStorageOptions = azureDeploymentStorageOptions ?? throw new ArgumentNullException(nameof(azureDeploymentStorageOptions));
        }

        string IAzureStorageArtifactsOptions.BaseUrlOverride => azureDeploymentStorageOptions.BaseUrlOverride;

        string IAzureStorageArtifactsOptions.ConnectionString => azureDeploymentStorageOptions.ConnectionString;
    }
}

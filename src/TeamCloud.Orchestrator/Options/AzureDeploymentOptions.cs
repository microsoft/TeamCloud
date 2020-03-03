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
        private readonly AzureDeploymentStorageOptions azureDeploymentStorageOptions;

        public AzureDeploymentOptions(AzureDeploymentStorageOptions azureDeploymentStorageOptions)
        {
            this.azureDeploymentStorageOptions = azureDeploymentStorageOptions ?? throw new ArgumentNullException(nameof(azureDeploymentStorageOptions));
        }

        public string BaseUrlOverride => azureDeploymentStorageOptions.BaseUrlOverride;

        public string ConnectionString => azureDeploymentStorageOptions.ConnectionString;
    }
}

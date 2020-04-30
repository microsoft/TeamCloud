/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Azure.Deployment
{
    public interface IAzureDeploymentOptions
    {
        string DefaultLocation { get; }
    }

    internal sealed class AzureDeploymentOptions : IAzureDeploymentOptions
    {
        public static IAzureDeploymentOptions Default => new AzureDeploymentOptions();

        private AzureDeploymentOptions() { }

        public string DefaultLocation { get; } = Environment.GetEnvironmentVariable("REGION_NAME");
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Azure.Deployment
{
    public interface IAzureDeploymentOptions
    {
    }

    public sealed class AzureDeploymentOptions : IAzureDeploymentOptions
    {
        public static IAzureDeploymentOptions Default => new AzureDeploymentOptions();

        private AzureDeploymentOptions() { }
    }
}

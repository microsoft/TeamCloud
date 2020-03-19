/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Diagnostics.CodeAnalysis;

namespace TeamCloud.Azure.Deployment
{
    [SuppressMessage("Design", "CA1040:Avoid empty interfaces")]
    public interface IAzureDeploymentOptions
    { }

    public sealed class AzureDeploymentOptions : IAzureDeploymentOptions
    {
        public static IAzureDeploymentOptions Default => new AzureDeploymentOptions();

        private AzureDeploymentOptions() { }
    }
}

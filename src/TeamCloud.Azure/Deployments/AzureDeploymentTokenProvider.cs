/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;

namespace TeamCloud.Azure.Deployments
{
    public interface IAzureDeploymentTokenProvider
    {
        Task<string> AcquireToken(Guid deploymentId, IAzureDeploymentArtifactsProvider azureDeploymentArtifactsProvider);
    }
}

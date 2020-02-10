/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;

namespace TeamCloud.Azure.Deployment
{
    public interface IAzureDeploymentArtifactsProvider
    {
        Task<IAzureDeploymentArtifactsContainer> UploadArtifactsAsync(Guid deploymentId, AzureDeploymentTemplate azureDeploymentTemplate);

        Task<string> DownloadArtifactAsync(Guid deploymentId, string artifactName);
    }
}

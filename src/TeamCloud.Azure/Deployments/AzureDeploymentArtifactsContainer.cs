/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Azure.Deployments
{
    public interface IAzureDeploymentArtifactsContainer
    {
        string Location { get; }

        string Token { get; }
    }
}

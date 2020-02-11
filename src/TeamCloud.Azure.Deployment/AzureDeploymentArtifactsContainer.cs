/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Azure.Deployment
{
    public interface IAzureDeploymentArtifactsContainer
    {
        string Location { get; }

        string Token { get; }
    }
}

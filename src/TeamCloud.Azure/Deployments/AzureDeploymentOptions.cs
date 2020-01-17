/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Azure.Deployments
{
    public interface IAzureDeploymentOptions
    {
        public string Region { get; }
        public string BaseUrl { get; }
    }
}

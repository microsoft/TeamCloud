/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Azure.Deployments.Providers
{
    public interface IAzureStorageArtifactsOptions
    {
        string BaseUrl { get; }

        string ConnectionString { get; }
    }
}

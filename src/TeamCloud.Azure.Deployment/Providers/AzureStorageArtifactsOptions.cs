/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Azure.Deployment.Providers;

public interface IAzureStorageArtifactsOptions
{
    string BaseUrlOverride { get; }

    string ConnectionString { get; }
}

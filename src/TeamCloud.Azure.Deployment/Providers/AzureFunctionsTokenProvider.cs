/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using TeamCloud.Http;

namespace TeamCloud.Azure.Deployment.Providers;

public sealed class AzureFunctionsTokenProvider : IAzureDeploymentTokenProvider
{
    private readonly IAzureStorageArtifactsOptions azureStorageArtifactsOptions;
    private readonly string functionName;

    public AzureFunctionsTokenProvider(IAzureStorageArtifactsOptions azureStorageArtifactsOptions, string functionName)
    {
        this.azureStorageArtifactsOptions = azureStorageArtifactsOptions ?? throw new ArgumentNullException(nameof(azureStorageArtifactsOptions));
        this.functionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
    }

    public async Task<string> AcquireToken(Guid deploymentId, IAzureDeploymentArtifactsProvider azureDeploymentArtifactsProvider)
    {
        var hostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

        // there is no need to do a key lookup when running on localhost
        // function apps running on localhost don't use api keys at all

        if (hostname.StartsWith("localhost", StringComparison.OrdinalIgnoreCase))
            return null;

        var json = await $"https://{hostname}/admin/functions/{functionName}/keys"
            .GetJObjectAsync()
            .ConfigureAwait(false);

        return json.SelectToken("keys[0].value")?.ToString();
    }

}

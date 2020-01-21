/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using TeamCloud.Azure.Deployments;
using TeamCloud.Azure.Deployments.Providers;
using TeamCloud.Orchestrator.API;

namespace TeamCloud.Orchestrator.Providers
{
    public class AzureDeploymentTokenProvider : IAzureDeploymentTokenProvider
    {
        private readonly IAzureStorageArtifactsOptions azureStorageArtifactsOptions;

        public AzureDeploymentTokenProvider(IAzureStorageArtifactsOptions azureStorageArtifactsOptions)
        {
            this.azureStorageArtifactsOptions = azureStorageArtifactsOptions ?? throw new ArgumentNullException(nameof(azureStorageArtifactsOptions));
        }

        public async Task<string> AcquireToken(Guid deploymentId, IAzureDeploymentArtifactsProvider azureDeploymentArtifactsProvider)
        {
            var hostname = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

            // there is no need to do a key lookup when running on localhost
            // function apps running on localhost don't use api keys at all

            if (hostname.StartsWith("localhost", StringComparison.OrdinalIgnoreCase))
                return null;

            var json = await $"https://{hostname}/admin/functions/{nameof(ArtifactTrigger)}/keys"
                .GetJObjectAsync()
                .ConfigureAwait(false);

            return json.SelectToken("keys[0].value")?.ToString();
        }

    }
}

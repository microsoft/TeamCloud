/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;

namespace TeamCloud.Azure.Deployments
{
    public interface IAzureDeploymentArtifactsStorage
    {
        Task<IAzureDeploymentArtifactsContainer> CreateContainerAsync(Guid deploymentId, IAzureDeploymentTemplate azureDeploymentTemplate);
    }    

    public class AzureDeploymentArtifactsStorage : IAzureDeploymentArtifactsStorage
    {
        private const string DEPLOYMENT_CONTAINER_NAME = "deployments";

        private static readonly ConcurrentDictionary<string, Lazy<BlobServiceClient>> BlobServiceClientCache 
            = new ConcurrentDictionary<string, Lazy<BlobServiceClient>>();

        private static Lazy<BlobServiceClient> GetBlobServiceClientFactory(string connectionString)
            => BlobServiceClientCache.GetOrAdd(connectionString, key => new Lazy<BlobServiceClient>(() => new BlobServiceClient(connectionString)));

        private readonly IAzureDeploymentArtifactsOptions azureDeploymentArtifactsOptions;

        public AzureDeploymentArtifactsStorage(IAzureDeploymentArtifactsOptions azureDeploymentArtifactsOptions)
        {
            this.azureDeploymentArtifactsOptions = azureDeploymentArtifactsOptions ?? throw new ArgumentNullException(nameof(azureDeploymentArtifactsOptions));
        }

        public async Task<IAzureDeploymentArtifactsContainer> CreateContainerAsync(Guid deploymentId, IAzureDeploymentTemplate azureDeploymentTemplate)
        {
            var initialize = azureDeploymentTemplate.LinkedTemplates?.Any() ?? false;

            var container = new Container()
            {
                Location = initialize
                    ? await UploadTemplatesAsync(deploymentId, azureDeploymentTemplate.LinkedTemplates).ConfigureAwait(false)
                    : default(string),

                Token = initialize
                    ? await CreateSasTokenAsync(deploymentId).ConfigureAwait(false)
                    : default(string)
            };

            return container;
        }

        private async Task<string> UploadTemplatesAsync(Guid deploymentId, IDictionary<string, string> templates)
        {
            var blobServiceClientFactory = GetBlobServiceClientFactory(azureDeploymentArtifactsOptions.ConnectionString);
            var blobServiceClientInitialized = blobServiceClientFactory.IsValueCreated;

            var deploymentContainerClient = blobServiceClientFactory.Value.GetBlobContainerClient(DEPLOYMENT_CONTAINER_NAME);
            if (!blobServiceClientInitialized) _ = await deploymentContainerClient.CreateIfNotExistsAsync().ConfigureAwait(false);

            Task.WaitAll(templates.Select(template => UploadTeamplateAsync($"{deploymentId}/{template.Key}", template.Value)).ToArray());

            return deploymentContainerClient.Uri.ToString();

            async Task UploadTeamplateAsync(string filePath, string fileContent)
            {
                using var blobStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));

                await deploymentContainerClient
                    .GetBlobClient(filePath)
                    .UploadAsync(blobStream)
                    .ConfigureAwait(false);
            }
        }

        private async Task<string> CreateSasTokenAsync(Guid deploymentId)
        {
            var blobServiceClientFactory = GetBlobServiceClientFactory(azureDeploymentArtifactsOptions.ConnectionString);

            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = DEPLOYMENT_CONTAINER_NAME,
                Resource = "c", // container level access
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(1)
            };

            sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

            var sasKey = await blobServiceClientFactory.Value
                .GetUserDelegationKeyAsync(sasBuilder.StartsOn, sasBuilder.ExpiresOn)
                .ConfigureAwait(false);

            return sasBuilder.ToSasQueryParameters(sasKey, blobServiceClientFactory.Value.AccountName).ToString();
        }

        public sealed class Container : IAzureDeploymentArtifactsContainer
        {
            public string Location { get; internal set; }

            public string Token { get; internal set; }
        }
    }
}

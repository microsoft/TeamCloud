/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace TeamCloud.Azure.Deployments.Providers
{
    public class AzureStorageArtifactsProvider : IAzureDeploymentArtifactsProvider
    {
        private const string DEPLOYMENT_CONTAINER_NAME = "deployments";

        private static readonly ConcurrentDictionary<string, Lazy<CloudBlobContainer>> DeploymentContainerCache
            = new ConcurrentDictionary<string, Lazy<CloudBlobContainer>>();

        private static Lazy<CloudBlobContainer> GetDeploymentContainer(string connectionString)
            => DeploymentContainerCache.GetOrAdd(connectionString, key => new Lazy<CloudBlobContainer>(() => CloudStorageAccount.Parse(key).CreateCloudBlobClient().GetContainerReference(DEPLOYMENT_CONTAINER_NAME)));

        private readonly IAzureStorageArtifactsOptions azureStorageArtifactsOptions;
        private readonly IAzureDeploymentTokenProvider azureDeploymentTokenProvider;

        public AzureStorageArtifactsProvider(IAzureStorageArtifactsOptions azureStorageArtifactsOptions, IAzureDeploymentTokenProvider azureDeploymentTokenProvider = null)
        {
            this.azureStorageArtifactsOptions = azureStorageArtifactsOptions ?? throw new ArgumentNullException(nameof(AzureStorageArtifactsProvider.azureStorageArtifactsOptions));
            this.azureDeploymentTokenProvider = azureDeploymentTokenProvider;
        }

        public async Task<IAzureDeploymentArtifactsContainer> UploadArtifactsAsync(Guid deploymentId, AzureDeploymentTemplate azureDeploymentTemplate)
        {
            var container = new Container();

            if (azureDeploymentTemplate.LinkedTemplates?.Any() ?? false)
            {
                var location = await UploadTemplatesAsync(deploymentId, azureDeploymentTemplate.LinkedTemplates)
                    .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(azureStorageArtifactsOptions.BaseUrl))
                    location = azureStorageArtifactsOptions.BaseUrl.AppendPathSegment(deploymentId).ToString();

                container.Location = $"{location.TrimEnd('/')}/";

                container.Token = azureDeploymentTokenProvider is null
                    ? await CreateSasTokenAsync(deploymentId).ConfigureAwait(false)
                    : await azureDeploymentTokenProvider.AcquireToken(deploymentId, this).ConfigureAwait(false);
            }

            return container;
        }

        private async Task<string> UploadTemplatesAsync(Guid deploymentId, IDictionary<string, string> templates)
        {
            var deploymentContainer = GetDeploymentContainer(azureStorageArtifactsOptions.ConnectionString);

            if (!deploymentContainer.IsValueCreated)
                await deploymentContainer.Value.CreateIfNotExistsAsync().ConfigureAwait(false);

            var uploadTasks = templates.Select(template => deploymentContainer.Value
                .GetBlockBlobReference($"{deploymentId}/{template.Key}")
                .UploadTextAsync(template.Value));

            Task.WaitAll(uploadTasks.ToArray());

            return deploymentContainer.Value.Uri.ToString()
                .AppendPathSegment(deploymentId.ToString())
                .ToString();
        }

        private async Task<string> CreateSasTokenAsync(Guid deploymentId)
        {
            var deploymentContainer = GetDeploymentContainer(azureStorageArtifactsOptions.ConnectionString);

            if (!deploymentContainer.IsValueCreated)
                await deploymentContainer.Value.CreateIfNotExistsAsync().ConfigureAwait(false);

            var adHocPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(1),
                Permissions = SharedAccessBlobPermissions.Read
            };

            return deploymentContainer.Value.GetSharedAccessSignature(adHocPolicy, null);
        }

        public async Task<string> DownloadArtifactAsync(Guid deploymentId, string artifactName)
        {
            var deploymentContainer = GetDeploymentContainer(azureStorageArtifactsOptions.ConnectionString);

            if (deploymentContainer.IsValueCreated)
            {
                var artifactBlob = deploymentContainer.Value.GetBlockBlobReference($"{deploymentId}/{artifactName}");
                var artifactExists = await artifactBlob.ExistsAsync().ConfigureAwait(false);

                if (artifactExists)
                    return await artifactBlob.DownloadTextAsync().ConfigureAwait(false);
            }

            return null;
        }

        public sealed class Container : IAzureDeploymentArtifactsContainer
        {
            public string Location { get; internal set; }

            public string Token { get; internal set; }
        }
    }
}

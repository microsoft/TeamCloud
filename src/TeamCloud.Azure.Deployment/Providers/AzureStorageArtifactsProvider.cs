/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace TeamCloud.Azure.Deployment.Providers
{
    public class AzureStorageArtifactsProvider : AzureDeploymentArtifactsProvider
    {
        private const string DEPLOYMENT_CONTAINER_NAME = "deployments";

        private readonly IAzureStorageArtifactsOptions azureStorageArtifactsOptions;
        private readonly IAzureDeploymentTokenProvider azureDeploymentTokenProvider;
        private readonly Lazy<CloudBlobContainer> deploymentContainer;

        public AzureStorageArtifactsProvider(IAzureStorageArtifactsOptions azureStorageArtifactsOptions, IAzureDeploymentTokenProvider azureDeploymentTokenProvider = null)
        {
            this.azureStorageArtifactsOptions = azureStorageArtifactsOptions ?? throw new ArgumentNullException(nameof(AzureStorageArtifactsProvider.azureStorageArtifactsOptions));
            this.azureDeploymentTokenProvider = azureDeploymentTokenProvider;

            deploymentContainer = new Lazy<CloudBlobContainer>(() => CloudStorageAccount
                .Parse(azureStorageArtifactsOptions.ConnectionString)
                .CreateCloudBlobClient().GetContainerReference(DEPLOYMENT_CONTAINER_NAME));
        }

        public override async Task<IAzureDeploymentArtifactsContainer> UploadArtifactsAsync(Guid deploymentId, AzureDeploymentTemplate azureDeploymentTemplate)
        {
            if (azureDeploymentTemplate is null)
                throw new ArgumentNullException(nameof(azureDeploymentTemplate));

            var container = new Container();

            if (azureDeploymentTemplate.LinkedTemplates?.Any() ?? false)
            {
                var location = await UploadTemplatesAsync(deploymentId, azureDeploymentTemplate.LinkedTemplates)
                    .ConfigureAwait(false);

                if (!string.IsNullOrEmpty(azureStorageArtifactsOptions.BaseUrlOverride)
                    && Uri.IsWellFormedUriString(azureStorageArtifactsOptions.BaseUrlOverride, UriKind.Absolute))
                {
                    location = azureStorageArtifactsOptions.BaseUrlOverride.AppendPathSegment(deploymentId).ToString();
                }
                else if (azureStorageArtifactsOptions.ConnectionString.IsDevelopmentStorageConnectionString())
                {
                    // if the artifact storage connection string points to a development storage (Azure Storage Emulator)
                    // a BaseUrlOverride must be given as the storage is not publicly accessable in this case.
                    // in this case it's required to use a tool like ngrok to make a local endpoint publicly available
                    // that deliveres artifacts requested by the ARM deployment.

                    throw new NotSupportedException($"Using development storage (Azure Storage Emulator) without a {nameof(azureStorageArtifactsOptions.BaseUrlOverride)} is not supported");
                }

                container.Location = $"{location.TrimEnd('/')}/";

                container.Token = azureDeploymentTokenProvider is null
                    ? await CreateSasTokenAsync().ConfigureAwait(false)
                    : await azureDeploymentTokenProvider.AcquireToken(deploymentId, this).ConfigureAwait(false);
            }

            return container;
        }

        private async Task<string> UploadTemplatesAsync(Guid deploymentId, IDictionary<string, string> templates)
        {
            if (!deploymentContainer.IsValueCreated)
                await deploymentContainer.Value.CreateIfNotExistsAsync()
                    .ConfigureAwait(false);

            var uploadTasks = templates.Select(template => deploymentContainer.Value
                .GetBlockBlobReference($"{deploymentId}/{template.Key}")
                .UploadTextAsync(template.Value));

            await uploadTasks
                .WhenAll()
                .ConfigureAwait(false);

            return deploymentContainer.Value.Uri.AbsoluteUri
                .AppendPathSegment(deploymentId.ToString())
                .ToString();
        }

        [SuppressMessage("Security", "CA5377:Use Container Level Access Policy", Justification = "SasToken authentication is required.")]
        private async Task<string> CreateSasTokenAsync()
        {
            if (!deploymentContainer.IsValueCreated)
                await deploymentContainer.Value.CreateIfNotExistsAsync().ConfigureAwait(false);

            var adHocPolicy = new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddDays(1),
                Permissions = SharedAccessBlobPermissions.Read
            };

            return deploymentContainer.Value.GetSharedAccessSignature(adHocPolicy, null);
        }

        public override async Task<string> DownloadArtifactAsync(Guid deploymentId, string artifactName)
        {
            if (!deploymentContainer.IsValueCreated)
                await deploymentContainer.Value.CreateIfNotExistsAsync().ConfigureAwait(false);

            var artifactBlob = deploymentContainer.Value.GetBlockBlobReference($"{deploymentId}/{artifactName}");
            var artifactExists = await artifactBlob.ExistsAsync().ConfigureAwait(false);

            if (artifactExists)
                return await artifactBlob.DownloadTextAsync().ConfigureAwait(false);

            return null;
        }

        internal sealed class Container : IAzureDeploymentArtifactsContainer
        {
            public string Location { get; internal set; }

            public string Token { get; internal set; }
        }
    }
}

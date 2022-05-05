/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Flurl;
using TeamCloud.Azure.Storage;

namespace TeamCloud.Azure.Deployment.Providers;

public class AzureStorageArtifactsProvider : AzureDeploymentArtifactsProvider
{
    private const string DEPLOYMENT_CONTAINER_NAME = "deployments";

    private readonly IStorageService storage;
    private readonly IAzureStorageArtifactsOptions azureStorageArtifactsOptions;
    private readonly IAzureDeploymentTokenProvider azureDeploymentTokenProvider;

    public AzureStorageArtifactsProvider(IStorageService storageService, IAzureStorageArtifactsOptions azureStorageArtifactsOptions, IAzureDeploymentTokenProvider azureDeploymentTokenProvider = null)
    {
        this.storage = storageService ?? throw new ArgumentNullException(nameof(storageService));
        this.azureStorageArtifactsOptions = azureStorageArtifactsOptions ?? throw new ArgumentNullException(nameof(AzureStorageArtifactsProvider.azureStorageArtifactsOptions));
        this.azureDeploymentTokenProvider = azureDeploymentTokenProvider;
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
            else if (azureStorageArtifactsOptions.ConnectionString.Contains("UseDevelopmentStorage"))
            {
                // if the artifact storage connection string points to a development storage (Azure Storage Emulator)
                // a BaseUrlOverride must be given as the storage is not publicly accessable in this case.
                // in this case it's required to use a tool like ngrok to make a local endpoint publicly available
                // that deliveres artifacts requested by the ARM deployment.

                throw new NotSupportedException($"Using development storage (Azure Storage Emulator) without a {nameof(azureStorageArtifactsOptions.BaseUrlOverride)} is not supported");
            }

            container.Location = $"{location.TrimEnd('/')}/";

            container.Token = azureDeploymentTokenProvider is null
                ? await CreateSasTokenAsync()
                : await azureDeploymentTokenProvider.AcquireToken(deploymentId, this).ConfigureAwait(false);
        }

        return container;
    }

    private async Task<string> UploadTemplatesAsync(Guid deploymentId, IDictionary<string, string> templates)
    {
        var blobContainerClient = await storage.Blobs
            .GetBlobContainerClientAsync(azureStorageArtifactsOptions.ConnectionString, DEPLOYMENT_CONTAINER_NAME)
            .ConfigureAwait(false);

        var uploadTasks = templates.Select(template =>
            blobContainerClient.GetBlobClient($"{deploymentId}/{template.Key}")
            .UploadAsync(BinaryData.FromString(template.Value), overwrite: true)
        );

        await uploadTasks
            .WhenAll()
            .ConfigureAwait(false);

        return blobContainerClient.GetBlobClient($"{deploymentId}").Uri.AbsoluteUri;
    }

    [SuppressMessage("Security", "CA5377:Use Container Level Access Policy", Justification = "SasToken authentication is required.")]
    private async Task<string> CreateSasTokenAsync()
    {
        var blobContainerClient = await storage.Blobs
            .GetBlobContainerClientAsync(azureStorageArtifactsOptions.ConnectionString, DEPLOYMENT_CONTAINER_NAME)
            .ConfigureAwait(false);

        var sasUri = blobContainerClient.GenerateSasUri(BlobContainerSasPermissions.Read, DateTimeOffset.UtcNow.AddDays(1));

        return sasUri.PathAndQuery;
    }

    public override async Task<string> DownloadArtifactAsync(Guid deploymentId, string artifactName)
    {
        var blobClient = await storage.Blobs
            .GetBlobClientAsync(azureStorageArtifactsOptions.ConnectionString, DEPLOYMENT_CONTAINER_NAME, $"{deploymentId}/{artifactName}")
            .ConfigureAwait(false);

        var artifactExists = await blobClient.ExistsAsync().ConfigureAwait(false);

        if (artifactExists)
        {
            var blob = await blobClient.DownloadContentAsync().ConfigureAwait(false);
            return blob.Value.Content.ToString();
        }

        return null;
    }

    internal sealed class Container : IAzureDeploymentArtifactsContainer
    {
        public string Location { get; internal set; }

        public string Token { get; internal set; }
    }
}

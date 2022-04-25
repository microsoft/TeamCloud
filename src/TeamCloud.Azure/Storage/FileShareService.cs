/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.ResourceManager.Storage;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Sas;
using Flurl;

namespace TeamCloud.Azure.Storage;

public interface IFileShareService
{
    public Task<ShareServiceClient> GetShareServiceClientAsync(string accountResourceId, CancellationToken cancellationToken = default);
    public Task<ShareClient> GetShareClientAsync(string accountResourceId, string shareName, CancellationToken cancellationToken = default);
    public Task<ShareDirectoryClient> GetShareDirectoryClientAsync(string accountResourceId, string shareName, string directoryPath, bool ensureDirectoryExists = false, CancellationToken cancellationToken = default);
    public Task<ShareFileClient> GetShareFileClientAsync(string accountResourceId, string shareName, string directoryPath, string fileName, bool ensureDirectroyExists = false, CancellationToken cancellationToken = default);
    public Task<Uri> GetShareFileSasUriAsync(string accountResourceId, string shareName, string filePath, ShareFileSasPermissions permissions, DateTimeOffset expiresOn, CancellationToken cancellationToken = default);
}

public class FileShareService : IFileShareService
{
    private readonly IArmService arm;
    public FileShareService(IArmService arm)
    {
        this.arm = arm ?? throw new ArgumentNullException(nameof(arm));
    }

    private readonly ConcurrentDictionary<string, ShareServiceClient> shareServiceClientMap = new(StringComparer.OrdinalIgnoreCase);

    public async Task<ShareServiceClient> GetShareServiceClientAsync(string accountResourceId, CancellationToken cancellationToken = default)
    {
        if (!shareServiceClientMap.TryGetValue(accountResourceId, out var shareServiceClient))
        {
            var resourceIdentifier = new ResourceIdentifier(accountResourceId);

            var keysResponse = await arm
                .GetArmClient(resourceIdentifier.SubscriptionId)
                .GetStorageAccountResource(resourceIdentifier)
                .GetKeysAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            var key = keysResponse.Value.Keys.First().Value;

            var connectionString = $"DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName={resourceIdentifier.Name};AccountKey={key}";

            shareServiceClient = new ShareServiceClient(connectionString);

            shareServiceClientMap[accountResourceId] = shareServiceClient;
        }

        return shareServiceClient;
    }

    public async Task<ShareClient> GetShareClientAsync(string accountResourceId, string shareName, CancellationToken cancellationToken = default)
    {
        var shareServiceClient = await GetShareServiceClientAsync(accountResourceId, cancellationToken)
            .ConfigureAwait(false);

        return shareServiceClient.GetShareClient(shareName);
    }

    public async Task<ShareDirectoryClient> GetShareDirectoryClientAsync(string accountResourceId, string shareName, string directoryPath, bool ensureDirectoryExists = false, CancellationToken cancellationToken = default)
    {
        var shareClient = await GetShareClientAsync(accountResourceId, shareName, cancellationToken)
            .ConfigureAwait(false);

        var directoryNames = directoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

        var directoryClient = shareClient.GetDirectoryClient(directoryNames[0]);

        if (ensureDirectoryExists)
        {
            await directoryClient
                .CreateIfNotExistsAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        if (directoryNames.Length > 1)
        {
            for (int i = 1; i < directoryNames.Length; i++)
            {
                directoryClient = directoryClient.GetSubdirectoryClient(directoryNames[i]);

                if (ensureDirectoryExists)
                {
                    await directoryClient
                        .CreateIfNotExistsAsync(cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
            }
        }

        return directoryClient;
    }

    public async Task<ShareFileClient> GetShareFileClientAsync(string accountResourceId, string shareName, string directoryPath, string fileName, bool ensureDirectroyExists = false, CancellationToken cancellationToken = default)
    {
        var directoryClient = await GetShareDirectoryClientAsync(accountResourceId, shareName, directoryPath, ensureDirectroyExists, cancellationToken)
            .ConfigureAwait(false);

        return directoryClient.GetFileClient(fileName);
    }

    public async Task<Uri> GetShareFileSasUriAsync(string accountResourceId, string shareName, string filePath, ShareFileSasPermissions permissions, DateTimeOffset expiresOn, CancellationToken cancellationToken = default)
    {
        var resourceIdentifier = new ResourceIdentifier(accountResourceId);

        var storageAccountResponse = await arm
            .GetArmClient(resourceIdentifier.SubscriptionId)
            .GetStorageAccountResource(resourceIdentifier)
            .GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var storageAccount = storageAccountResponse.Value;

        var shareFileUri = storageAccount.Data.PrimaryEndpoints.File
            .AppendPathSegment(shareName)
            .AppendPathSegments(filePath.Split('/', StringSplitOptions.RemoveEmptyEntries))
            .ToUri();

        var storageAccountKeys = await storageAccount
            .GetKeysAsync(cancellationToken)
            .ConfigureAwait(false);

        var sharedKeyCredential = new StorageSharedKeyCredential(storageAccount.Data.Name, storageAccountKeys.Value.Keys.First().Value);

        var client = new ShareFileClient(shareFileUri, sharedKeyCredential);

        return client.GenerateSasUri(permissions, expiresOn);
    }
}
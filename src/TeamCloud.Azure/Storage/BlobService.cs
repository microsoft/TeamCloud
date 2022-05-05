/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace TeamCloud.Azure.Storage;

public interface IBlobService
{
    Task<BlobContainerClient> GetBlobContainerClientAsync(string connectionString, string containerName, bool ensureContainer = true, CancellationToken cancellationToken = default);
    Task<BlobClient> GetBlobClientAsync(string connectionString, string containerName, string blobName, bool ensureContainer = true, CancellationToken cancellationToken = default);
}

public class BlobService : IBlobService
{
    private readonly ConcurrentDictionary<string, BlobContainerClient> blobContainerClientMap = new(StringComparer.OrdinalIgnoreCase);

    public async Task<BlobContainerClient> GetBlobContainerClientAsync(string connectionString, string containerName, bool ensureContainer = true, CancellationToken cancellationToken = default)
    {
        var accountName = connectionString
            .Split(';')
            .FirstOrDefault(p => p.StartsWith("AccountName=", StringComparison.OrdinalIgnoreCase))?
            .Split('=')
            .LastOrDefault();

        var key = $"{accountName ?? connectionString}{containerName.ToLowerInvariant()}";

        if (!blobContainerClientMap.TryGetValue(key, out var blobContainerClient))
        {
            blobContainerClient = new BlobContainerClient(connectionString, containerName);

            if (ensureContainer)
                await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

            blobContainerClientMap[key] = blobContainerClient;
        }

        return blobContainerClient;
    }

    public async Task<BlobClient> GetBlobClientAsync(string connectionString, string containerName, string blobName, bool ensureContainer = true, CancellationToken cancellationToken = default)
    {
        var blobContainerClient = await GetBlobContainerClientAsync(connectionString, containerName, ensureContainer, cancellationToken)
            .ConfigureAwait(false);

        return blobContainerClient.GetBlobClient(blobName);
    }
}
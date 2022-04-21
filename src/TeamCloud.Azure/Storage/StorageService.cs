/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.ResourceManager.Storage;

namespace TeamCloud.Azure.Storage;

public interface IStorageService
{
    IBlobService Blobs { get; }
    ITableService Tables { get; }
    IFileShareService FileShares { get; }
    Task<StorageAccountResource> GetStorageAccountAsync(string resourceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetStorageAccountKeysAsync(string resourceId, CancellationToken cancellationToken = default);
    Task<string> GetStorageAccountConnectionStringAsync(string resourceId, string key = null, CancellationToken cancellationToken = default);
}

public class StorageService : IStorageService
{
    private readonly IArmService arm;
    private readonly IBlobService blobs;
    private readonly ITableService tables;
    private readonly IFileShareService fileShares;

    public StorageService(IArmService arm, IBlobService blobs, ITableService tables, IFileShareService fileShares)
    {
        this.blobs = blobs ?? throw new ArgumentNullException(nameof(blobs));
        this.tables = tables ?? throw new ArgumentNullException(nameof(tables));
        this.fileShares = fileShares ?? throw new ArgumentNullException(nameof(fileShares));
        this.arm = arm ?? throw new ArgumentNullException(nameof(arm));
    }

    public IBlobService Blobs => blobs;

    public ITableService Tables => tables;

    public IFileShareService FileShares => fileShares;


    private StorageAccountResource GetStorageAccount(string resourceId)
    {
        var resourceIdentifier = new ResourceIdentifier(resourceId);

        return arm
            .GetArmClient(resourceIdentifier.SubscriptionId)
            .GetStorageAccountResource(resourceIdentifier);
    }

    public async Task<StorageAccountResource> GetStorageAccountAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        var response = await GetStorageAccount(resourceId)
            .GetAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return response.Value;
    }

    public async Task<IEnumerable<string>> GetStorageAccountKeysAsync(string resourceId, CancellationToken cancellationToken = default)
    {
        var storageKeys = await GetStorageAccount(resourceId)
            .GetKeysAsync(cancellationToken)
            .ConfigureAwait(false);

        return storageKeys.Value.Keys
            .Select(k => k.Value);
    }

    public async Task<string> GetStorageAccountConnectionStringAsync(string resourceId, string key = null, CancellationToken cancellationToken = default)
    {
        var resourceIdentifier = new ResourceIdentifier(resourceId);

        if (string.IsNullOrEmpty(key))
        {
            var storageKeys = await GetStorageAccountKeysAsync(resourceIdentifier, cancellationToken)
                .ConfigureAwait(false);

            key = storageKeys.First();
        }

        return $"DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName={resourceIdentifier.Name};AccountKey={key}";
    }
}
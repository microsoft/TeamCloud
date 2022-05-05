/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using Azure.Storage.Sas;
using Flurl;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using TeamCloud.Azure.Resources.Utilities;

namespace TeamCloud.Azure.Resources.Typed;

public sealed class AzureStorageAccountResource : AzureTypedResource
{
    private readonly AsyncLazy<IStorageAccount> storageInstance;

    internal AzureStorageAccountResource(string resourceId) : base("Microsoft.Storage/storageAccounts", resourceId)
    {
        storageInstance = new AsyncLazy<IStorageAccount>(() => GetStorageAsync());
    }

    private async Task<IStorageAccount> GetStorageAsync()
    {
        var session = await AzureResourceService.AzureSessionService
            .CreateSessionAsync(ResourceId.SubscriptionId)
            .ConfigureAwait(false);

        return await session.StorageAccounts
            .GetByIdAsync(ResourceId.ToString())
            .ConfigureAwait(false);
    }

    // public async Task<IEnumerable<string>> GetKeysAsync()
    // {
    //     var storage = await storageInstance
    //         .ConfigureAwait(false);

    //     var storageKeys = await storage
    //         .GetKeysAsync()
    //         .ConfigureAwait(false);

    //     return storageKeys
    //         .Select(k => k.Value);
    // }

    // public async Task<string> GetConnectionStringAsync()
    // {
    //     var storage = await storageInstance
    //         .ConfigureAwait(false);

    //     var storageKeys = await storage
    //         .GetKeysAsync()
    //         .ConfigureAwait(false);

    //     var storageCredentials = new StorageCredentials(storage.Name, storageKeys.First().Value);

    //     return new CloudStorageAccount(storageCredentials, true).ToString(true);
    // }

    // public async Task EnsureDirectoryPathAsync(string shareName, string directoryPath)
    // {
    //     if (string.IsNullOrEmpty(shareName))
    //         throw new ArgumentException($"'{nameof(shareName)}' cannot be null or empty.", nameof(shareName));

    //     if (string.IsNullOrEmpty(directoryPath))
    //         throw new ArgumentException($"'{nameof(directoryPath)}' cannot be null or empty.", nameof(directoryPath));

    //     var directoryNames = directoryPath.Split('/');

    //     for (int i = 0; i < directoryNames.Length; i++)
    //     {
    //         var path = string.Join('/', directoryNames.Take(i + 1));

    //         await CreateShareDirectoryClientAsync(shareName, path)
    //             .ContinueWith(client => client.Result.CreateIfNotExistsAsync()).Unwrap()
    //             .ConfigureAwait(false);
    //     }
    // }

    // public async Task<ShareClient> CreateShareClientAsync(string shareName, bool useSharedKey = false, ShareClientOptions options = null)
    // {
    //     if (string.IsNullOrWhiteSpace(shareName))
    //         throw new ArgumentException($"'{nameof(shareName)}' cannot be null or whitespace.", nameof(shareName));

    //     if (useSharedKey)
    //     {
    //         var storage = await storageInstance
    //             .ConfigureAwait(false);

    //         var shareUri = storage.EndPoints.Primary.File
    //             .AppendPathSegment(shareName)
    //             .ToUri();

    //         return await GetKeysAsync()
    //             .ContinueWith(keys => new StorageSharedKeyCredential(storage.Name, keys.Result.First()), TaskContinuationOptions.OnlyOnRanToCompletion)
    //             .ContinueWith(cred => new ShareClient(shareUri, cred.Result, options))
    //             .ConfigureAwait(false);
    //     }

    //     return await GetConnectionStringAsync()
    //         .ContinueWith((connectionString) => new ShareClient(connectionString.Result, shareName, options), TaskContinuationOptions.OnlyOnRanToCompletion)
    //         .ConfigureAwait(false);
    // }

    // public async Task<Uri> CreateShareSasUriAsync(string shareName, string directoryPath, ShareSasPermissions permissions, DateTimeOffset expiresOn)
    // {
    //     if (string.IsNullOrWhiteSpace(shareName))
    //         throw new ArgumentException($"'{nameof(shareName)}' cannot be null or whitespace.", nameof(shareName));

    //     if (string.IsNullOrWhiteSpace(directoryPath))
    //         throw new ArgumentException($"'{nameof(directoryPath)}' cannot be null or whitespace.", nameof(directoryPath));

    //     var client = await CreateShareClientAsync(shareName, useSharedKey: true)
    //         .ConfigureAwait(false);

    //     return client.GenerateSasUri(permissions, expiresOn);
    // }

    // public async Task<ShareDirectoryClient> CreateShareDirectoryClientAsync(string shareName, string directoryPath, bool useSharedKey = false, ShareClientOptions options = null)
    // {
    //     if (string.IsNullOrWhiteSpace(shareName))
    //         throw new ArgumentException($"'{nameof(shareName)}' cannot be null or whitespace.", nameof(shareName));

    //     if (string.IsNullOrWhiteSpace(directoryPath))
    //         throw new ArgumentException($"'{nameof(directoryPath)}' cannot be null or whitespace.", nameof(directoryPath));

    //     if (useSharedKey)
    //     {
    //         var storage = await storageInstance
    //             .ConfigureAwait(false);

    //         var shareFileUri = storage.EndPoints.Primary.File
    //             .AppendPathSegment(shareName)
    //             .AppendPathSegments(directoryPath.Split('/', StringSplitOptions.RemoveEmptyEntries))
    //             .ToUri();

    //         return await GetKeysAsync()
    //             .ContinueWith(keys => new StorageSharedKeyCredential(storage.Name, keys.Result.First()), TaskContinuationOptions.OnlyOnRanToCompletion)
    //             .ContinueWith(cred => new ShareDirectoryClient(shareFileUri, cred.Result, options))
    //             .ConfigureAwait(false);
    //     }

    //     return await GetConnectionStringAsync()
    //         .ContinueWith((connectionString) => new ShareDirectoryClient(connectionString.Result, shareName, directoryPath, options), TaskContinuationOptions.OnlyOnRanToCompletion)
    //         .ConfigureAwait(false);
    // }

    // public async Task<Uri> CreateShareDirectorySasUriAsync(string shareName, string directoryPath, ShareFileSasPermissions permissions, DateTimeOffset expiresOn)
    // {
    //     if (string.IsNullOrWhiteSpace(shareName))
    //         throw new ArgumentException($"'{nameof(shareName)}' cannot be null or whitespace.", nameof(shareName));

    //     if (string.IsNullOrWhiteSpace(directoryPath))
    //         throw new ArgumentException($"'{nameof(directoryPath)}' cannot be null or whitespace.", nameof(directoryPath));

    //     var client = await CreateShareDirectoryClientAsync(shareName, directoryPath, useSharedKey: true)
    //         .ConfigureAwait(false);

    //     return client.GenerateSasUri(permissions, expiresOn);
    // }

    // public async Task<ShareFileClient> CreateShareFileClientAsync(string shareName, string filePath, bool useSharedKey = false, ShareClientOptions options = null)
    // {
    //     if (string.IsNullOrWhiteSpace(shareName))
    //         throw new ArgumentException($"'{nameof(shareName)}' cannot be null or whitespace.", nameof(shareName));

    //     if (string.IsNullOrWhiteSpace(filePath))
    //         throw new ArgumentException($"'{nameof(filePath)}' cannot be null or whitespace.", nameof(filePath));

    //     if (useSharedKey)
    //     {
    //         var storage = await storageInstance
    //             .ConfigureAwait(false);

    //         var shareFileUri = storage.EndPoints.Primary.File
    //             .AppendPathSegment(shareName)
    //             .AppendPathSegments(filePath.Split('/', StringSplitOptions.RemoveEmptyEntries))
    //             .ToUri();

    //         return await GetKeysAsync()
    //             .ContinueWith(keys => new StorageSharedKeyCredential(storage.Name, keys.Result.First()), TaskContinuationOptions.OnlyOnRanToCompletion)
    //             .ContinueWith(cred => new ShareFileClient(shareFileUri, cred.Result, options))
    //             .ConfigureAwait(false);
    //     }

    //     return await GetConnectionStringAsync()
    //         .ContinueWith((connectionString) => new ShareFileClient(connectionString.Result, shareName, filePath, options), TaskContinuationOptions.OnlyOnRanToCompletion)
    //         .ConfigureAwait(false);
    // }

    // public async Task<Uri> CreateShareFileSasUriAsync(string shareName, string filePath, ShareFileSasPermissions permissions, DateTimeOffset expiresOn)
    // {
    //     if (string.IsNullOrWhiteSpace(shareName))
    //         throw new ArgumentException($"'{nameof(shareName)}' cannot be null or whitespace.", nameof(shareName));

    //     if (string.IsNullOrWhiteSpace(filePath))
    //         throw new ArgumentException($"'{nameof(filePath)}' cannot be null or whitespace.", nameof(filePath));

    //     var client = await CreateShareFileClientAsync(shareName, filePath, useSharedKey: true)
    //         .ConfigureAwait(false);

    //     return client.GenerateSasUri(permissions, expiresOn);
    // }
}

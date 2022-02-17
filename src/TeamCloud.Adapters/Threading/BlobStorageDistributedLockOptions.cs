/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Adapters.Threading;

public sealed class BlobStorageDistributedLockOptions : IBlobStorageDistributedLockOptions
{
    public static IBlobStorageDistributedLockOptions Default
        => new BlobStorageDistributedLockOptions();

    private BlobStorageDistributedLockOptions()
    { }

    public string ConnectionString
        => Environment.GetEnvironmentVariable("AzureWebJobsStorage");
}

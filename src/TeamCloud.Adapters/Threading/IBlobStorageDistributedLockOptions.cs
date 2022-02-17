/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Adapters.Threading;

public interface IBlobStorageDistributedLockOptions
{
    public string ConnectionString { get; }
}

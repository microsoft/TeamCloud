/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Adapters.Threading;

public interface IBlobStorageDistributeLockOptions
{
    public string ConnectionString { get; }
}

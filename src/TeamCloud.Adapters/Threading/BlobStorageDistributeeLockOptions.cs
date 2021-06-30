/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;

namespace TeamCloud.Adapters.Threading
{
    public sealed class BlobStorageDistributeeLockOptions : IBlobStorageDistributeLockOptions
    {
        public static IBlobStorageDistributeLockOptions Default
            => new BlobStorageDistributeeLockOptions();

        private BlobStorageDistributeeLockOptions()
        { }

        public string ConnectionString
            => Environment.GetEnvironmentVariable("AzureWebJobsStorage");
    }
}

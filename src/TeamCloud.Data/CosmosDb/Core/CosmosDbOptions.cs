/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Data.CosmosDb.Core
{
    public interface ICosmosDbOptions
    {
        string TenantName { get; }

        string DatabaseName { get; }

        string ConnectionString { get; }
    }
}

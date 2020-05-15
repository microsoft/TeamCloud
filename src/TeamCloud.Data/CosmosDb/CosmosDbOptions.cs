/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

namespace TeamCloud.Data.CosmosDb
{
    public interface ICosmosDbOptions
    {
        string DatabaseName { get; }

        string ConnectionString { get; }
    }
}

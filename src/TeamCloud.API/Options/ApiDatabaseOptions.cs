/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;
using TeamCloud.Data.CosmosDb;
using TeamCloud.Data.CosmosDb.Core;

namespace TeamCloud.API.Options
{
    [Options]
    public sealed class ApiDatabaseOptions : ICosmosDbOptions
    {
        private readonly CosmosDbOptions cosmosDbOptions;

        public ApiDatabaseOptions(CosmosDbOptions cosmosDbOptions)
        {
            this.cosmosDbOptions = cosmosDbOptions;
        }

        public string TenantName => DatabaseName;

        public string DatabaseName => cosmosDbOptions.DatabaseName;

        public string ConnectionString => cosmosDbOptions.ConnectionString;
    }
}

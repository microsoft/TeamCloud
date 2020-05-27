/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;
using TeamCloud.Data.CosmosDb;

namespace TeamCloud.API.Options
{
    [Options]
    public sealed class ApiDatabaseOptions : ICosmosDbOptions
    {
        private readonly CosmosDbOptions cosmosDbOptions;

        public ApiDatabaseOptions(CosmosDbOptions cosmosDbOptions)
        {
            this.cosmosDbOptions = cosmosDbOptions ?? throw new System.ArgumentNullException(nameof(cosmosDbOptions));
        }

        string ICosmosDbOptions.DatabaseName => cosmosDbOptions.DatabaseName;

        string ICosmosDbOptions.ConnectionString => cosmosDbOptions.ConnectionString;
    }
}

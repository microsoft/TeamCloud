/**
*  Copyright (c) Microsoft Corporation.
*  Licensed under the MIT License.
*/

using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;
using TeamCloud.Data.CosmosDb;

namespace TeamCloud.Orchestrator.Options
{
    [Options()]
    public class DatabaseOptionsProxy : ICosmosDbOptions
    {
        private readonly CosmosDbOptions cosmosDbOptions;

        public DatabaseOptionsProxy(CosmosDbOptions cosmosDbOptions)
        {
            this.cosmosDbOptions = cosmosDbOptions ?? throw new System.ArgumentNullException(nameof(cosmosDbOptions));
        }

        string ICosmosDbOptions.DatabaseName => cosmosDbOptions.DatabaseName;

        string ICosmosDbOptions.ConnectionString => cosmosDbOptions.ConnectionString;
    }
}

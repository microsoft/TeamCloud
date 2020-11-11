/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Configuration;
using TeamCloud.Configuration.Options;
using TeamCloud.Data.CosmosDb.Core;

namespace TeamCloud.Orchestrator.Options
{
    [Options]
    public sealed class TeamCloudDatabaseOptions : ICosmosDbOptions
    {
        private readonly CosmosDbOptions cosmosDbOptions;

        public TeamCloudDatabaseOptions(CosmosDbOptions cosmosDbOptions)
        {
            this.cosmosDbOptions = cosmosDbOptions ?? throw new System.ArgumentNullException(nameof(cosmosDbOptions));
        }

        public string DatabaseName => cosmosDbOptions.DatabaseName;

        public string ConnectionString => cosmosDbOptions.ConnectionString;
    }
}

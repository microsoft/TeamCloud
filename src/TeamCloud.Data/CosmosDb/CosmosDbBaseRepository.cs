/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Azure.Cosmos;
using Azure.Cosmos.Fluent;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.CosmosDb
{
    public abstract class CosmosDbBaseRepository
    {
        private readonly ICosmosDbOptions cosmosOptions;
        private readonly Lazy<CosmosClient> cosmosClient;
        private readonly ConcurrentDictionary<Type, Lazy<Container>> cosmosContainers = new ConcurrentDictionary<Type, Lazy<Container>>();

        protected CosmosDbBaseRepository(ICosmosDbOptions cosmosOptions)
        {
            this.cosmosOptions = cosmosOptions ?? throw new ArgumentNullException(nameof(cosmosOptions));

            cosmosClient = new Lazy<CosmosClient>(() => new CosmosClientBuilder(cosmosOptions.ConnectionString)
                .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase })
                .Build());
        }

        protected async Task<Database> GetDatabaseAsync()
        {
            if (cosmosClient.IsValueCreated)
                return cosmosClient.Value.GetDatabase(cosmosOptions.DatabaseName);

            var response = await cosmosClient.Value
                .CreateDatabaseIfNotExistsAsync(cosmosOptions.DatabaseName)
                .ConfigureAwait(false);

            return response.Database;
        }

        protected async Task<Container> GetContainerAsync<T>()
            where T : IContainerDocument, new()
        {
            var database = await GetDatabaseAsync().ConfigureAwait(false);

            var container = cosmosContainers.GetOrAdd(typeof(T), containerType
                => new Lazy<Container>(() => database.GetContainer(containerType.Name)));

            if (!container.IsValueCreated)
                await database
                    .CreateContainerIfNotExistsAsync(new ContainerProperties(typeof(T).Name, IContainerDocument.PartitionKeyPath))
                    .ConfigureAwait(false);

            return container.Value;
        }

    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using TeamCloud.Data.Utilities;
using TeamCloud.Model.Data;
using static Microsoft.Azure.Cosmos.Container;

namespace TeamCloud.Data.CosmosDb
{
    public abstract class CosmosDbBaseRepository<T>
        where T : IContainerDocument, new()
    {
        private readonly ICosmosDbOptions cosmosOptions;
        private readonly Lazy<CosmosClient> cosmosClient;

        private readonly ConcurrentDictionary<Type, AsyncLazy<(Container, ChangeFeedProcessor)>> cosmosContainers = new ConcurrentDictionary<Type, AsyncLazy<(Container, ChangeFeedProcessor)>>();

        protected CosmosDbBaseRepository(ICosmosDbOptions cosmosOptions)
        {
            this.cosmosOptions = cosmosOptions ?? throw new ArgumentNullException(nameof(cosmosOptions));

            cosmosClient = new Lazy<CosmosClient>(() => new CosmosClient(cosmosOptions.ConnectionString, new CosmosClientOptions()
            {
                SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
            }));
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

        protected async Task<Container> GetContainerAsync()
        {
            var database = await GetDatabaseAsync()
                .ConfigureAwait(false);

            var containerEntry = cosmosContainers.GetOrAdd(typeof(T), containerType
                => new AsyncLazy<(Container, ChangeFeedProcessor)>(() => CreateContainerAsync(database, typeof(T), HandleChangesAsync)));

            var (container, processor) = await containerEntry
                .ConfigureAwait(false);

            return container;
        }

        private static async Task<(Container, ChangeFeedProcessor)> CreateContainerAsync(Database database, Type containerType, ChangesHandler<T> changesHandler)
        {
            var containerBuilder = database.DefineContainer(typeof(T).Name, IContainerDocument.PartitionKeyPath);
            var containerKeys = (new T()).UniqueKeys;

            if (containerKeys.Any())
            {
                foreach (var containerKey in containerKeys)
                {
                    containerBuilder = containerBuilder
                        .WithUniqueKey()
                        .Path(containerKey)
                        .Attach();
                }
            }

            _ = await containerBuilder
                .CreateIfNotExistsAsync()
                .ConfigureAwait(false);

            if (changesHandler is null)
            {
                // no changes handler was provided. so we don't need to
                // create change feed processor and return just the container

                return (database.GetContainer(containerType.Name), null);
            }

            _ = await database
                .DefineContainer($"{typeof(T).Name}-leases", "/id")
                .CreateIfNotExistsAsync()
                .ConfigureAwait(false);

            var processorInstance = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")
                ?? $"{Environment.MachineName}-{Process.GetCurrentProcess().Id}";

            var processor = database.GetContainer(containerType.Name)
                .GetChangeFeedProcessorBuilder<T>(containerType.Name, changesHandler)
                .WithInstanceName($"{containerType.Name}-{processorInstance}")
                .WithLeaseContainer(database.GetContainer($"{typeof(T).Name}-leases"))
                .WithStartTime(DateTime.UtcNow)
                .Build();

            await processor
                .StartAsync()
                .ConfigureAwait(false);

            return (database.GetContainer(containerType.Name), processor);
        }

        protected virtual ChangesHandler<T> HandleChangesAsync { get; }
    }
}

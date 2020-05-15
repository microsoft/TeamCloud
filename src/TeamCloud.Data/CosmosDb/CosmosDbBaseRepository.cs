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
using TeamCloud.Model.Data;
using static Microsoft.Azure.Cosmos.Container;

namespace TeamCloud.Data.CosmosDb
{
    public abstract class CosmosDbBaseRepository<T>
        where T : IContainerDocument, new()
    {
        private readonly ICosmosDbOptions cosmosOptions;
        private readonly Lazy<CosmosClient> cosmosClient;
        private readonly ConcurrentDictionary<Type, Lazy<Container>> cosmosContainers = new ConcurrentDictionary<Type, Lazy<Container>>();
        private readonly ConcurrentDictionary<Type, Lazy<ChangeFeedProcessor>> cosmosProcessors = new ConcurrentDictionary<Type, Lazy<ChangeFeedProcessor>>();

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
            var database = await GetDatabaseAsync().ConfigureAwait(false);

            var containerFactory = cosmosContainers.GetOrAdd(typeof(T), containerType
                => new Lazy<Container>(() => database.GetContainer(containerType.Name)));

            var container = await containerFactory.InitializeAsync(async (container) =>
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

                if (HandleChangesAsync is null)
                {
                    // unfortunately the SDK doesn't offer a 
                    // function to check if a container exists
                    // in an elegant way. so we do it brute force

                    try
                    {
                        _ = await database
                            .GetContainer($"{typeof(T).Name}-leases")
                            .DeleteContainerAsync()
                            .ConfigureAwait(false);
                    }
                    catch
                    {
                        // swallow any exception
                    }
                }
                else
                {
                    // create the leases container that is 
                    // needed by the change feed processor

                    _ = await database
                        .DefineContainer($"{typeof(T).Name}-leases", "/id")
                        .CreateIfNotExistsAsync()
                        .ConfigureAwait(false);
                }

            }).ConfigureAwait(false);


            if (HandleChangesAsync != null)
            {
                // create the change feed processor

                var processorInstance = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")
                    ?? $"{Environment.MachineName}-{Process.GetCurrentProcess().Id}";

                var processorFactory = cosmosProcessors.GetOrAdd(typeof(T), containerType
                    => new Lazy<ChangeFeedProcessor>(() => container
                    .GetChangeFeedProcessorBuilder<T>(this.GetType().Name, HandleChangesAsync)
                    .WithInstanceName($"{this.GetType().Name}-{processorInstance}")
                    .WithLeaseContainer(database.GetContainer($"{typeof(T).Name}-leases"))
                    .WithStartTime(DateTime.UtcNow)
                    .Build()));

                // initialiaze the change feed processor

                _ = await processorFactory
                    .InitializeAsync((processor) => processor.StartAsync())
                    .ConfigureAwait(false);
            }

            return container;
        }

        protected virtual ChangesHandler<T> HandleChangesAsync { get; }
    }
}

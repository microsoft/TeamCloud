/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Cosmos;
using Azure.Cosmos.Fluent;
using Azure.Cosmos.Serialization;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.CosmosDb
{
    internal sealed class CosmosDbContainerFactory
    {
        private static readonly ConcurrentDictionary<string, CosmosDbContainerFactory> containerFactories = new ConcurrentDictionary<string, CosmosDbContainerFactory>();

        public static CosmosDbContainerFactory Get(ICosmosDbOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var key = $"{options.DatabaseName}@{options.ConnectionString}";

            return containerFactories.GetOrAdd(key, _ => new CosmosDbContainerFactory(options));
        }

        private readonly ICosmosDbOptions options;
        private readonly HashSet<Type> containers = new HashSet<Type>();
        private readonly Lazy<CosmosClient> client;

        private CosmosDbContainerFactory(ICosmosDbOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            client = new Lazy<CosmosClient>(() =>
            {
                var builder = new CosmosClientBuilder(options.ConnectionString)
                    .WithSerializerOptions(new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase });

                return builder.Build();
            });
        }

        public async Task<CosmosDatabase> GetDatabaseAsync()
        {
            if (client.IsValueCreated)
            {
                // our cosmos client was already created so the database should exist
                return client.Value.GetDatabase(options.DatabaseName);
            }
            else
            {
                // uninitialized cosmos client - ensure our database exists
                var response = await client.Value
                    .CreateDatabaseIfNotExistsAsync(options.DatabaseName)
                    .ConfigureAwait(false);

                return response.Database;
            }
        }

        public async Task<CosmosContainer> GetContainerAsync<T>()
            where T : IContainerDocument, new()
        {
            var database = await GetDatabaseAsync().ConfigureAwait(false);

            if (containers.Add(typeof(T)))
            {
                var containerResponse = await database
                    .CreateContainerIfNotExistsAsync(new ContainerProperties(typeof(T).Name, IContainerDocument.PartitionKeyPath))
                    .ConfigureAwait(false);

                return containerResponse.Container;
            }
            else
            {
                return database.GetContainer(typeof(T).Name);
            }
        }
    }
}

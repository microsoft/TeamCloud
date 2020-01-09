/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model;

namespace TeamCloud.Data.Cosmos
{
    internal sealed class CosmosContainerFactory
    {
        private static readonly ConcurrentDictionary<string, CosmosContainerFactory> containerFactories = new ConcurrentDictionary<string, CosmosContainerFactory>();

        public static CosmosContainerFactory Get(ICosmosOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var key = $"{options.AzureCosmosDBName}@{options.AzureCosmosDBConnection}";

            return containerFactories.GetOrAdd(key, _ => new CosmosContainerFactory(options));
        }

        private readonly ICosmosOptions options;
        private readonly HashSet<Type> containers = new HashSet<Type>();
        private readonly Lazy<CosmosClient> client;

        private CosmosContainerFactory(ICosmosOptions options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));

            client = new Lazy<CosmosClient>(() =>
            {
                var builder = new CosmosClientBuilder(options.AzureCosmosDBConnection)
                    .WithSerializerOptions(new CosmosSerializationOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase });

                return builder.Build();
            });
        }

        public async Task<Database> GetDatabaseAsync()
        {
            if (client.IsValueCreated)
            {
                // our cosmos client was already created so the database should exist
                return client.Value.GetDatabase(options.AzureCosmosDBName);
            }
            else
            {
                // uninitialized cosmos client - ensure our database exists
                var response = await client.Value
                    .CreateDatabaseIfNotExistsAsync(options.AzureCosmosDBName)
                    .ConfigureAwait(false);

                return response.Database;
            }
        }

        public async Task<Container> GetContainerAsync<T>()
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

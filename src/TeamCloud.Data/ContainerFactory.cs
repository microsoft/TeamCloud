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
using TeamCloud.Model;

namespace TeamCloud.Data
{
    internal sealed class ContainerFactory
    {
        private static readonly ConcurrentDictionary<string, ContainerFactory> containerFactories = new ConcurrentDictionary<string, ContainerFactory>();

        public static ContainerFactory Get(ICosmosOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var key = $"{options.AzureCosmosDBName}@{options.AzureCosmosDBConnection}";

            return containerFactories.GetOrAdd(key, _ => new ContainerFactory(options));
        }

        private readonly ICosmosOptions options;
        private readonly HashSet<Type> containers = new HashSet<Type>();
        private readonly Lazy<CosmosClient> client;

        private ContainerFactory(ICosmosOptions options)
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

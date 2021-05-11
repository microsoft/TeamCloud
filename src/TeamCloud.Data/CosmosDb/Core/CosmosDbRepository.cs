/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.Cosmos;
using TeamCloud.Data.Utilities;
using TeamCloud.Model.Common;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Data.CosmosDb.Core
{
    public interface ICosmosDbRepository
    {
        ICosmosDbOptions Options { get; }

        Type ContainerDocumentType { get; }
    }

    public abstract class CosmosDbRepository<T> : ICosmosDbRepository, IDocumentRepository<T>
        where T : class, IContainerDocument, new()
    {
        private readonly Lazy<CosmosClient> cosmosClient;
        private readonly ConcurrentDictionary<Type, AsyncLazy<(Container, ChangeFeedProcessor)>> cosmosContainers = new ConcurrentDictionary<Type, AsyncLazy<(Container, ChangeFeedProcessor)>>();
        private readonly IDocumentExpanderProvider expanderProvider;
        private readonly IDocumentSubscriptionProvider subscriptionProvider;

        protected CosmosDbRepository(ICosmosDbOptions options, IDocumentExpanderProvider expanderProvider = null, IDocumentSubscriptionProvider subscriptionProvider = null, IDataProtectionProvider dataProtectionProvider = null)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));

            this.expanderProvider = expanderProvider ?? NullExpanderProvider.Instance;
            this.subscriptionProvider = subscriptionProvider ?? NullSubscriptionProvider.Instance;

            cosmosClient = new Lazy<CosmosClient>(() => new CosmosClient(options.ConnectionString, new CosmosClientOptions()
            {
                Serializer = new CosmosDbSerializer(dataProtectionProvider)
            }));
        }

        public ICosmosDbOptions Options { get; }

        public Type ContainerDocumentType { get; } = typeof(T);

        protected QueryRequestOptions GetQueryRequestOptions(PartitionKey partitionKey)
            => new QueryRequestOptions { PartitionKey = partitionKey };

        protected QueryRequestOptions GetQueryRequestOptions(string partitionKeyValue)
            => GetQueryRequestOptions(GetPartitionKey(partitionKeyValue));

        protected QueryRequestOptions GetQueryRequestOptions(T containerDocument)
            => GetQueryRequestOptions(GetPartitionKey(containerDocument));

        protected PartitionKey GetPartitionKey(string partitionKeyValue)
            => new PartitionKey(partitionKeyValue);

        protected PartitionKey GetPartitionKey(T containerDocument)
        {
            if (containerDocument is null)
                throw new ArgumentNullException(nameof(containerDocument));

            var partitionKeyValue = PartitionKeyAttribute.GetValue(containerDocument);

            if (partitionKeyValue is null)
                throw new ArgumentException($"{typeof(T)} does not provide a partition key.");

            return GetPartitionKey(partitionKeyValue as string);
        }

        protected int GetSoftDeleteTTL()
        {
            var softDeleteTTL = SoftDeleteAttribute.GetSoftDeleteTTL<T>();

            if (!softDeleteTTL.HasValue)
                throw new ArgumentException($"{typeof(T)} does not provide a value soft delete TTL.");

            return softDeleteTTL.Value;
        }

        protected async Task<Database> GetDatabaseAsync()
        {
            if (cosmosClient.IsValueCreated)
                return cosmosClient.Value.GetDatabase(Options.DatabaseName);

            var response = await cosmosClient.Value
                .CreateDatabaseIfNotExistsAsync(Options.DatabaseName, ThroughputProperties.CreateAutoscaleThroughput(4000))
                .ConfigureAwait(false);

            return response.Database;
        }

        protected async Task<Container> GetContainerAsync()
        {
            var database = await GetDatabaseAsync()
                .ConfigureAwait(false);

            var containerEntry = cosmosContainers.GetOrAdd(typeof(T), containerType
                => new AsyncLazy<(Container, ChangeFeedProcessor)>(() => CreateContainerAsync(database, typeof(T)), LazyThreadSafetyMode.PublicationOnly));

            var (container, processor) = await containerEntry.Value.ConfigureAwait(false);

            return container;
        }

        private static async Task<(Container, ChangeFeedProcessor)> CreateContainerAsync(Database database, Type containerType)
        {
            var containerName = ContainerNameAttribute.GetNameOrDefault(containerType);
            var containerPartitionKey = PartitionKeyAttribute.GetPath(containerType, true);
            var containerUniqueKeys = UniqueKeyAttribute.GetPaths(containerType, true);
            var containerBuilder = database.DefineContainer(containerName, containerPartitionKey);

            if (containerUniqueKeys.Any())
            {
                foreach (var containerUniqueKey in containerUniqueKeys)
                {
                    containerBuilder = containerBuilder
                        .WithUniqueKey()
                        .Path(containerUniqueKey)
                        .Attach();
                }
            }

            if (typeof(ISoftDelete).IsAssignableFrom(containerType) && containerType.IsDefined(typeof(SoftDeleteAttribute), false))
            {
                containerBuilder = containerBuilder
                    .WithDefaultTimeToLive(-1);
            }

            _ = await containerBuilder
                .CreateIfNotExistsAsync()
                .ConfigureAwait(false);

            return (database.GetContainer(containerName), null);
        }

        public abstract Task<T> AddAsync(T document);

        public abstract Task<T> GetAsync(string partitionId, string documentId, bool expand = false);

        public abstract Task<T> SetAsync(T document);

        public abstract IAsyncEnumerable<T> ListAsync(string partitionId);

        public abstract Task<T> RemoveAsync(T document);

        protected async Task<IEnumerable<T>> NotifySubscribersAsync(IEnumerable<T> documents, DocumentSubscriptionEvent subscriptionEvent)
        {
            if (documents is null)
                throw new ArgumentNullException(nameof(documents));

            var tasks = documents
                .Select(document => NotifySubscribersAsync(document, subscriptionEvent));

            return await Task
                .WhenAll(tasks)
                .ConfigureAwait(false);
        }

        protected Task<T> NotifySubscribersAsync(T document, DocumentSubscriptionEvent subscriptionEvent)
        {
            if (document != null)
            {
                var tasks = subscriptionProvider
                    .GetSubscriptions(document)
                    .Where(subscriber => subscriber.CanHandle(document))
                    .Select(subscriber => subscriber.HandleAsync(document, subscriptionEvent));

                if (tasks.Any())
                {
                    return Task
                        .WhenAll(tasks)
                        .ContinueWith(t => document, TaskScheduler.Current);
                }
            }

            return Task.FromResult(document);
        }

        public virtual async Task<T> ExpandAsync(T document, bool includeOptional = false)
        {
            if (document is null)
                throw new ArgumentNullException(nameof(document));

            foreach (var expander in expanderProvider.GetExpanders(document, includeOptional))
                document = (T)await expander.ExpandAsync(document).ConfigureAwait(false);

            return document;
        }

        private class NullExpanderProvider : IDocumentExpanderProvider
        {
            public static readonly IDocumentExpanderProvider Instance = new NullExpanderProvider();

            public IEnumerable<IDocumentExpander> GetExpanders(IContainerDocument containerDocument, bool includeOptional)
                => Enumerable.Empty<IDocumentExpander>();
        }

        private class NullSubscriptionProvider : IDocumentSubscriptionProvider
        {
            public static readonly IDocumentSubscriptionProvider Instance = new NullSubscriptionProvider();

            public IEnumerable<IDocumentSubscription> GetSubscriptions(IContainerDocument containerDocument)
                => Enumerable.Empty<IDocumentSubscription>();
        }
    }
}

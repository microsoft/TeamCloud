/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading;
using System.Threading.Tasks;
using TeamCloud.Data.Caching;

namespace TeamCloud.Data.CosmosDb.Core
{
    public sealed class CosmosDbTestCache : IContainerDocumentCache
    {
        public static readonly IContainerDocumentCache Instance = new CosmosDbTestCache();

        private CosmosDbTestCache()
        { }

        public bool InMemory => true;

        public Task RefreshAsync(string key, CancellationToken token = default)
            => Task.CompletedTask;

        public Task RemoveAsync(string key, CancellationToken token = default)
            => Task.CompletedTask;

        Task<ContainerDocumentCacheEntry<T>> IContainerDocumentCache.GetAsync<T>(string key, CancellationToken token)
            => Task.FromResult(default(ContainerDocumentCacheEntry<T>));

        Task<ContainerDocumentCacheEntry<T>> IContainerDocumentCache.SetAsync<T>(string key, ContainerDocumentCacheEntry<T> value, ContainerDocumentCacheEntryOptions options, CancellationToken token)
            => Task.FromResult(value);
    }
}

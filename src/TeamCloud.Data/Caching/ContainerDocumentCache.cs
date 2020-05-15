/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.Caching
{
    public interface IContainerDocumentCache
    {
        bool InMemory { get; }

        Task<ContainerDocumentCacheEntry<T>> GetAsync<T>(string key, CancellationToken token = default)
            where T : class, IContainerDocument, new();

        Task<ContainerDocumentCacheEntry<T>> SetAsync<T>(string key, ContainerDocumentCacheEntry<T> value, ContainerDocumentCacheEntryOptions options, CancellationToken token = default)
            where T : class, IContainerDocument, new();

        Task RemoveAsync(string key, CancellationToken token = default);

        Task RefreshAsync(string key, CancellationToken token = default);

    }

    public class ContainerDocumentCache : IContainerDocumentCache
    {
        private readonly IDistributedCache cache;

        public ContainerDocumentCache(IDistributedCache cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public bool InMemory => cache is MemoryDistributedCache;

        public async Task<ContainerDocumentCacheEntry<T>> GetAsync<T>(string key, CancellationToken token)
            where T : class, IContainerDocument, new()
        {
            var json = await cache
                .GetStringAsync(key, token)
                .ConfigureAwait(false);

            return ContainerDocumentCacheEntry<T>.Deserialize(json);
        }

        public async Task<ContainerDocumentCacheEntry<T>> SetAsync<T>(string key, ContainerDocumentCacheEntry<T> value, ContainerDocumentCacheEntryOptions options, CancellationToken token)
            where T : class, IContainerDocument, new()
        {
            var json = ContainerDocumentCacheEntry<T>.Serialize(value);

            await cache
                .SetStringAsync(key, json, options, token)
                .ConfigureAwait(false);

            return value;
        }

        public Task RemoveAsync(string key, CancellationToken token = default)
            => cache.RemoveAsync(key, token);

        public Task RefreshAsync(string key, CancellationToken token = default)
            => cache.RefreshAsync(key, token);
    }
}

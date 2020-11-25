using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace TeamCloud.Git.Caching
{
    public sealed class RepositoryCache : IRepositoryCache
    {
        private readonly IDistributedCache cache;

        public RepositoryCache(IDistributedCache cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public bool InMemory => cache is MemoryDistributedCache;

        public Task<string> GetAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            return cache.GetStringAsync(endpoint, cancellationToken);
        }

        public Task SetAsync(string endpoint, string value, CancellationToken cancellationToken = default)
        {
            return cache.SetStringAsync(endpoint, value, new DistributedCacheEntryOptions() { SlidingExpiration = TimeSpan.FromDays(1) }, cancellationToken);
        }
    }
}

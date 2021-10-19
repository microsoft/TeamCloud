/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

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
            return cache.GetStringAsync(CreateKey(endpoint), cancellationToken);
        }

        public Task SetAsync(string endpoint, string value, CancellationToken cancellationToken = default)
        {
            return cache.SetStringAsync(CreateKey(endpoint), value, new DistributedCacheEntryOptions() { SlidingExpiration = TimeSpan.FromDays(1) }, cancellationToken);
        }

        private static string CreateKey(string endpoint)
        {
            // This fails because Cosmos decodes the uri when it adds the id as a scope for an access token
            // return System.Web.HttpUtility.UrlEncode(endpoint);\

            if (endpoint is null)
                throw new ArgumentNullException(nameof(endpoint));

            // so let's get hacky and do some custom stuff
            var key = endpoint
                .Replace("/", "[s]", StringComparison.Ordinal)
                .Replace("?", "[q]", StringComparison.Ordinal)
                .Replace("#", "[h]", StringComparison.Ordinal);

            return key;
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.Caching
{
    public static class ContainerDocumentCacheExtensions
    {
        public static async Task<ContainerDocumentCacheEntry<T>> GetOrCreateAsync<T>(this IContainerDocumentCache cache, string key, Func<string, Task<ContainerDocumentCacheEntry<T>>> factory, CancellationToken token = default)
            where T : class, IContainerDocument, new()
        {
            if (cache is null)
                throw new ArgumentNullException(nameof(cache));

            if (key is null)
                throw new ArgumentNullException(nameof(key));

            if (factory is null)
                throw new ArgumentNullException(nameof(factory));

            ContainerDocumentCacheEntry<T> cacheEntry;

            try
            {
                cacheEntry = await cache
                    .GetAsync<T>(key, token)
                    .ConfigureAwait(false);
            }
            catch
            {
                cacheEntry = null;
            }

            return cacheEntry ?? await factory(key).ConfigureAwait(false);
        }
    }
}

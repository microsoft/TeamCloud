using System;
using Microsoft.Extensions.Caching.Distributed;

namespace TeamCloud.Data.Caching
{
    public sealed class ContainerDocumentCacheEntryOptions
    {
        public static implicit operator DistributedCacheEntryOptions(ContainerDocumentCacheEntryOptions cosmosOptions) => cosmosOptions is null ? default : new DistributedCacheEntryOptions()
        {
            AbsoluteExpiration = cosmosOptions.AbsoluteExpiration,
            AbsoluteExpirationRelativeToNow = cosmosOptions.AbsoluteExpirationRelativeToNow,
            SlidingExpiration = cosmosOptions.SlidingExpiration
        };

        public DateTimeOffset? AbsoluteExpiration { get; set; }

        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }

    }
}

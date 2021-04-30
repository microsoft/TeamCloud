/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Caching.Memory;
using TeamCloud.Adapters.Utilities;

namespace TeamCloud.Adapters.Authorization
{
    public sealed class AuthorizationTokenClient : IAuthorizationTokenClient
    {
        public const string TableName = "AuthorizationToken";

        private readonly IAuthorizationTokenOptions options;
        private readonly IAuthorizationEntityResolver resolver;
        private readonly IMemoryCache cache;

        private readonly AsyncLazy<CloudTable> tableInstance;

        public AuthorizationTokenClient(IAuthorizationTokenOptions options, IAuthorizationEntityResolver resolver, IMemoryCache cache)
        {
            this.options = options ?? AuthorizationTokenOptions.Default;
            this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));

            tableInstance = new AsyncLazy<CloudTable>(async () =>
            {
                var table = CloudStorageAccount
                    .Parse(this.options.ConnectionString)
                    .CreateCloudTableClient()
                    .GetTableReference(TableName);

                await table
                    .CreateIfNotExistsAsync()
                    .ConfigureAwait(false);

                return table;
            });
        }

        public async Task<AuthorizationToken> GetAsync(string authId)
        {
            if (string.IsNullOrWhiteSpace(authId))
                throw new ArgumentException($"'{nameof(authId)}' cannot be null or whitespace.", nameof(authId));

            var table = await tableInstance.Value
                .ConfigureAwait(false);

            var partitionKey = await cache.GetOrCreateAsync($"{authId}@{this.GetType().FullName}", async cacheEntry =>
            {
                cacheEntry.SetSlidingExpiration(AuthorizationSession.DefaultTTL);

                var query = new TableQuery()
                    .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, authId))
                    .Select(new string[] { "PartitionKey" });

                var response = await table
                    .ExecuteQuerySegmentedAsync(query, null)
                    .ConfigureAwait(false);

                return response.Results.SingleOrDefault()?.PartitionKey;

            }).ConfigureAwait(false);


            if (!string.IsNullOrEmpty(partitionKey))
            {
                var response = await table
                    .ExecuteAsync(TableOperation.Retrieve(partitionKey, authId, resolver.Resolve))
                    .ConfigureAwait(false);

                var authorizationToken = response.Result as AuthorizationToken;

                return authorizationToken;
            }

            return null;
        }

        public async Task<AuthorizationToken> SetAsync(AuthorizationToken authorizationToken)
        {
            if (authorizationToken is null)
                throw new ArgumentNullException(nameof(authorizationToken));

            var table = await tableInstance.Value
                .ConfigureAwait(false);

            var response = await table
                .ExecuteAsync(TableOperation.InsertOrReplace(authorizationToken))
                .ConfigureAwait(false);

            return response.Result as AuthorizationToken;
        }
    }
}

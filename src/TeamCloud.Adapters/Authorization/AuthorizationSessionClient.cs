/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Caching.Memory;
using TeamCloud.Adapters.Utilities;

namespace TeamCloud.Adapters.Authorization
{

    public sealed class AuthorizationSessionClient : IAuthorizationSessionClient
    {
        public const string TableName = "AuthorizationSession";

        private readonly IAuthorizationSessionOptions options;
        private readonly IAuthorizationEntityResolver resolver;
        private readonly IMemoryCache cache;
        private readonly AsyncLazy<CloudTable> tableInstance;

        public AuthorizationSessionClient(IAuthorizationSessionOptions options, IAuthorizationEntityResolver resolver, IMemoryCache cache, IDataProtectionProvider dataProtectionProvider)
        {
            this.options = options ?? AuthorizationSessionOptions.Default;
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

        public async Task<AuthorizationSession> GetAsync(string authId)
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

                return response.Results
                    .SingleOrDefault(r => Type.GetType(r.PartitionKey, false) != null)?
                    .PartitionKey;

            }).ConfigureAwait(false);


            if (!string.IsNullOrEmpty(partitionKey))
            {
                var response = await table
                    .ExecuteAsync(TableOperation.Retrieve(partitionKey, authId, resolver.Resolve))
                    .ConfigureAwait(false);

                var authorizationSession = response.Result as AuthorizationSession;

                return (authorizationSession?.Active ?? false) ? authorizationSession : null;
            }

            return null;
        }

        public async Task<AuthorizationSession> SetAsync(AuthorizationSession authorizationSession)
        {
            if (authorizationSession is null)
                throw new ArgumentNullException(nameof(authorizationSession));

            var table = await tableInstance.Value
                .ConfigureAwait(false);

            var response = await table
                .ExecuteAsync(TableOperation.InsertOrReplace(authorizationSession))
                .ConfigureAwait(false);

            return response.Result as AuthorizationSession;
        }
    }
}

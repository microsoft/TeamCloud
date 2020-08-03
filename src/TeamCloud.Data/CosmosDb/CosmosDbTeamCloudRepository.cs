/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using TeamCloud.Data.Caching;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbTeamCloudRepository : CosmosDbRepository<TeamCloudInstanceDocument>, ITeamCloudRepository
    {
        private readonly IContainerDocumentCache cache;

        public CosmosDbTeamCloudRepository(ICosmosDbOptions cosmosOptions, IContainerDocumentCache cache)
            : base(cosmosOptions)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private async Task<ContainerDocumentCacheEntry<TeamCloudInstanceDocument>> SetCacheAsync(ItemResponse<TeamCloudInstanceDocument> response)
        {
            if (response is null)
            {
                await cache
                    .RemoveAsync(nameof(TeamCloudInstanceDocument))
                    .ConfigureAwait(false);

                return null;
            }
            else
            {
                return await cache
                    .SetAsync(nameof(TeamCloudInstanceDocument), new ContainerDocumentCacheEntry<TeamCloudInstanceDocument>(response), new ContainerDocumentCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) })
                    .ConfigureAwait(false);
            }
        }

        protected override Container.ChangesHandler<TeamCloudInstanceDocument> HandleChangesAsync
            => cache.InMemory
            ? base.HandleChangesAsync // in case we deal with an InMemory cache, we fall back to the default implementation
            : (IReadOnlyCollection<TeamCloudInstanceDocument> changes, CancellationToken cancellationToken) => SetCacheAsync(null);

        public Task<TeamCloudInstanceDocument> GetAsync()
            => GetAsync(false);

        public async Task<TeamCloudInstanceDocument> GetAsync(bool refresh)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            ContainerDocumentCacheEntry<TeamCloudInstanceDocument> cacheEntry = null;

            if (cache != null && !refresh)
            {
                var created = false;

                cacheEntry = await cache
                    .GetOrCreateAsync(nameof(TeamCloudInstanceDocument), (key) => { created = true; return FetchAsync(); })
                    .ConfigureAwait(false);

                if (!created && cache.InMemory)
                {
                    var cacheEntryCurrent = await FetchAsync(cacheEntry.ETag)
                        .ConfigureAwait(false);

                    return (cacheEntryCurrent ?? cacheEntry)?.Value;
                }
            }

            return (cacheEntry ?? await FetchAsync().ConfigureAwait(false))?.Value
                ?? await SetAsync(new TeamCloudInstanceDocument() { Id = Options.TenantName }).ConfigureAwait(false);

            async Task<ContainerDocumentCacheEntry<TeamCloudInstanceDocument>> FetchAsync(string currentETag = default)
            {
                var measure = Stopwatch.StartNew();

                try
                {
                    var options = new ItemRequestOptions()
                    {
                        IfNoneMatchEtag = currentETag
                    };

                    var response = await container
                        .ReadItemAsync<TeamCloudInstanceDocument>(Options.TenantName, new PartitionKey(Options.TenantName), options)
                        .ConfigureAwait(false);

                    return await SetCacheAsync(response)
                        .ConfigureAwait(false);
                }
                catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.NotFound)
                {
                    return null; // the requested document does not exist - return null instead of bubbling the exception
                }
                catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.NotModified)
                {
                    return null; // the requested document exist but the provided etag is still equal to the current version in the db - return null instead of bubbling the exception
                }
                finally
                {
                    Debug.WriteLine($"Fetching '{nameof(TeamCloudInstanceDocument)}' (ETag: {currentETag ?? "EMPTY"}) took {measure.ElapsedMilliseconds} msec.");
                }
            }
        }

        public async Task<TeamCloudInstanceDocument> SetAsync(TeamCloudInstanceDocument teamCloudInstance)
        {
            if (teamCloudInstance is null)
                throw new ArgumentNullException(nameof(teamCloudInstance));

            await teamCloudInstance
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(teamCloudInstance, new PartitionKey(Options.TenantName))
                .ConfigureAwait(false);

            var cacheEntry = await SetCacheAsync(response)
                .ConfigureAwait(false);

            return cacheEntry.Value;
        }

    }
}

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
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbOrganizationRepository : CosmosDbRepository<OrganizationDocument>, IOrganizationRepository
    {
        private readonly IContainerDocumentCache cache;

        public CosmosDbOrganizationRepository(ICosmosDbOptions cosmosOptions, IContainerDocumentCache cache)
            : base(cosmosOptions)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private async Task<ContainerDocumentCacheEntry<OrganizationDocument>> SetCacheAsync(ItemResponse<OrganizationDocument> response)
        {
            if (response is null)
            {
                await cache
                    .RemoveAsync(nameof(OrganizationDocument))
                    .ConfigureAwait(false);

                return null;
            }
            else
            {
                return await cache
                    .SetAsync(nameof(OrganizationDocument), new ContainerDocumentCacheEntry<OrganizationDocument>(response), new ContainerDocumentCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) })
                    .ConfigureAwait(false);
            }
        }

        protected override Container.ChangesHandler<OrganizationDocument> HandleChangesAsync
            => cache.InMemory
            ? base.HandleChangesAsync // in case we deal with an InMemory cache, we fall back to the default implementation
            : (IReadOnlyCollection<OrganizationDocument> changes, CancellationToken cancellationToken) => SetCacheAsync(null);

        public Task<OrganizationDocument> GetAsync()
            => GetAsync(false);

        public async Task<OrganizationDocument> GetAsync(bool refresh)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            ContainerDocumentCacheEntry<OrganizationDocument> cacheEntry = null;

            if (cache != null && !refresh)
            {
                var created = false;

                cacheEntry = await cache
                    .GetOrCreateAsync(nameof(OrganizationDocument), (key) => { created = true; return FetchAsync(); })
                    .ConfigureAwait(false);

                if (!created && cache.InMemory)
                {
                    var cacheEntryCurrent = await FetchAsync(cacheEntry.ETag)
                        .ConfigureAwait(false);

                    return (cacheEntryCurrent ?? cacheEntry)?.Value;
                }
            }

            return (cacheEntry ?? await FetchAsync().ConfigureAwait(false))?.Value
                ?? await SetAsync(new OrganizationDocument() { Id = Options.TenantName }).ConfigureAwait(false);

            async Task<ContainerDocumentCacheEntry<OrganizationDocument>> FetchAsync(string currentETag = default)
            {
                var measure = Stopwatch.StartNew();

                try
                {
                    var options = new ItemRequestOptions()
                    {
                        IfNoneMatchEtag = currentETag
                    };

                    var response = await container
                        .ReadItemAsync<OrganizationDocument>(Options.TenantName, new PartitionKey(Options.TenantName), options)
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
                    Debug.WriteLine($"Fetching '{nameof(OrganizationDocument)}' (ETag: {currentETag ?? "EMPTY"}) took {measure.ElapsedMilliseconds} msec.");
                }
            }
        }

        public async Task<OrganizationDocument> SetAsync(OrganizationDocument organization)
        {
            if (organization is null)
                throw new ArgumentNullException(nameof(organization));

            await organization
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(organization, new PartitionKey(Options.TenantName))
                .ConfigureAwait(false);

            var cacheEntry = await SetCacheAsync(response)
                .ConfigureAwait(false);

            return cacheEntry.Value;
        }

    }
}

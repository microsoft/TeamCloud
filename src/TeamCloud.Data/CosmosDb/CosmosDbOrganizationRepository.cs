/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbOrganizationRepository : CosmosDbRepository<Organization>, IOrganizationRepository
    {
        private readonly IMemoryCache cache;

        public CosmosDbOrganizationRepository(ICosmosDbOptions options, IEnumerable<IDocumentExpander> expanders, IMemoryCache cache)
            : base(options, expanders)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }


        public async Task<string> ResolveIdAsync(string tenant, string identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            var key = $"{tenant}_{identifier}";

            if (!cache.TryGetValue(key, out string id))
            {
                var organization = await GetAsync(tenant, identifier)
                    .ConfigureAwait(false);

                id = organization?.Id;

                if (!string.IsNullOrEmpty(id))
                    cache.Set(key, cache, TimeSpan.FromMinutes(10));
            }

            return id;
        }

        public override async Task<Organization> AddAsync(Organization organization)
        {
            if (organization is null)
                throw new ArgumentNullException(nameof(organization));

            await organization
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(organization, GetPartitionKey(organization))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public override async Task<Organization> GetAsync(string tenant, string identifier, bool expand = false)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            Organization organization = null;

            try
            {
                var response = await container
                    .ReadItemAsync<Organization>(identifier, GetPartitionKey(tenant))
                    .ConfigureAwait(false);

                organization = response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                var query = new QueryDefinition($"SELECT * FROM o WHERE o.slug = '{identifier}'");

                var queryIterator = container
                    .GetItemQueryIterator<Organization>(query, requestOptions: GetQueryRequestOptions(tenant));

                if (queryIterator.HasMoreResults)
                {
                    var queryResults = await queryIterator
                        .ReadNextAsync()
                        .ConfigureAwait(false);

                    organization = queryResults.FirstOrDefault();
                }
            }

            if (expand)
            {
                organization = await ExpandAsync(organization)
                    .ConfigureAwait(false);
            }

            return organization;
        }

        public override async IAsyncEnumerable<Organization> ListAsync(string tenant)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM o");

            var queryIterator = container
                .GetItemQueryIterator<Organization>(query, requestOptions: GetQueryRequestOptions(tenant));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async IAsyncEnumerable<Organization> ListAsync(string tenant, IEnumerable<string> identifiers)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var search = "'" + string.Join("', '", identifiers) + "'";
            var query = new QueryDefinition($"SELECT * FROM o WHERE o.id IN ({search}) OR o.slug IN ({search}) OR o.displayName in ({search})");

            var queryIterator = container
                .GetItemQueryIterator<Organization>(query, requestOptions: GetQueryRequestOptions(tenant));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var org in queryResponse)
                    yield return org;
            }
        }

        public override async Task<Organization> RemoveAsync(Organization organization)
        {
            if (organization is null)
                throw new ArgumentNullException(nameof(organization));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<Organization>(organization.Id, GetPartitionKey(organization))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }

        public override async Task<Organization> SetAsync(Organization organization)
        {
            if (organization is null)
                throw new ArgumentNullException(nameof(organization));

            await organization
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(organization, GetPartitionKey(organization))
                .ConfigureAwait(false);

            return response.Resource;
        }
    }
}

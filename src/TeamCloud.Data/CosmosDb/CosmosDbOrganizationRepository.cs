/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using TeamCloud.Data.Caching;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbOrganizationRepository : CosmosDbRepository<Organization>, IOrganizationRepository
    {
        private readonly IMemoryCache idCache;

        public CosmosDbOrganizationRepository(ICosmosDbOptions cosmosOptions, IMemoryCache idCache)
            : base(cosmosOptions)
        {
            this.idCache = idCache ?? throw new ArgumentNullException(nameof(idCache));
        }


        public async Task<string> ResolveIdAsync(string identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            var key = $"{Options.TenantName}_{identifier}";

            if (!idCache.TryGetValue(key, out string id))
            {
                var organization = await GetAsync(identifier)
                    .ConfigureAwait(false);

                id = organization?.Id;

                if (!string.IsNullOrEmpty(id))
                    idCache.Set(key, idCache, TimeSpan.FromMinutes(10));
            }

            return id;
        }

        public async Task<Organization> AddAsync(Organization organization)
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

        public async Task<Organization> GetAsync(string identifier)
        {
            if (identifier is null)
                throw new ArgumentNullException(nameof(identifier));

            // if (!Guid.TryParse(id, out var idParsed))
            //     throw new ArgumentException("Value is not a valid GUID", nameof(id));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            Organization organization = null;

            try
            {
                var response = await container
                    .ReadItemAsync<Organization>(identifier, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                organization = response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                var query = new QueryDefinition($"SELECT * FROM o WHERE o.slug = '{identifier}'");

                var queryIterator = container
                    .GetItemQueryIterator<Organization>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

                if (queryIterator.HasMoreResults)
                {
                    var queryResults = await queryIterator
                        .ReadNextAsync()
                        .ConfigureAwait(false);

                    organization = queryResults.FirstOrDefault();
                }
            }

            return organization;
        }

        public async IAsyncEnumerable<Organization> ListAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM o");

            var queryIterator = container
                .GetItemQueryIterator<Organization>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async Task<Organization> RemoveAsync(Organization organization)
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

        public async Task<Organization> SetAsync(Organization organization)
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

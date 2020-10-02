/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbProviderRepository : CosmosDbRepository<ProviderDocument>, IProviderRepository
    {
        public CosmosDbProviderRepository(ICosmosDbOptions cosmosOptions)
            : base(cosmosOptions)
        { }

        public async Task<ProviderDocument> AddAsync(ProviderDocument provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            await provider
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(provider, new PartitionKey(Options.TenantName))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public async Task<ProviderDocument> GetAsync(string id)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ProviderDocument>(id, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<ProviderDocument> SetAsync(ProviderDocument provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            await provider
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(provider, new PartitionKey(Options.TenantName))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public IAsyncEnumerable<ProviderDocument> ListAsync(bool includeServiceProviders = true)
            => ListWithQueryAsync(includeServiceProviders ? $"SELECT * FROM p" : $"SELECT * FROM p WHERE p.type != '{ProviderType.Service}'");

        public IAsyncEnumerable<ProviderDocument> ListAsync(ProviderType providerType)
            => ListWithQueryAsync($"SELECT * FROM p WHERE p.type = '{providerType}'");

        public IAsyncEnumerable<ProviderDocument> ListAsync(IEnumerable<string> ids)
        {
            var search = "'" + string.Join("', '", ids) + "'";
            return ListWithQueryAsync($"SELECT * FROM p WHERE p.id IN ({search})");
        }

        private async IAsyncEnumerable<ProviderDocument> ListWithQueryAsync(string queryString)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition(queryString);

            var queryIterator = container
                .GetItemQueryIterator<ProviderDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                {
                    yield return queryResult;
                }
            }
        }

        public async Task<ProviderDocument> RemoveAsync(ProviderDocument provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ProviderDocument>(provider.Id, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }
    }
}

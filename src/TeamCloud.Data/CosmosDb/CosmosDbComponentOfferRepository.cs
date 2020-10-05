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
    public class CosmosDbComponentOfferRepository : CosmosDbRepository<ComponentOfferDocument>, IComponentOfferRepository
    {
        public CosmosDbComponentOfferRepository(ICosmosDbOptions cosmosOptions)
            : base(cosmosOptions)
        { }

        public async Task<ComponentOfferDocument> AddAsync(ComponentOfferDocument componentOffer)
        {
            if (componentOffer is null)
                throw new ArgumentNullException(nameof(componentOffer));

            await componentOffer
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(componentOffer, new PartitionKey(Options.TenantName))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public async Task<ComponentOfferDocument> GetAsync(string id)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ComponentOfferDocument>(id, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<ComponentOfferDocument> SetAsync(ComponentOfferDocument componentOffer)
        {
            if (componentOffer is null)
                throw new ArgumentNullException(nameof(componentOffer));

            await componentOffer
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(componentOffer, new PartitionKey(Options.TenantName))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public IAsyncEnumerable<ComponentOfferDocument> ListAsync()
            => ListWithQueryAsync($"SELECT * FROM o");

        public IAsyncEnumerable<ComponentOfferDocument> ListAsync(string providerId)
            => ListWithQueryAsync($"SELECT * FROM o WHERE o.providerId = '{providerId}'");

        public IAsyncEnumerable<ComponentOfferDocument> ListAsync(IEnumerable<string> providerIds)
        {
            var search = "'" + string.Join("', '", providerIds) + "'";
            return ListWithQueryAsync($"SELECT * FROM o WHERE o.providerId IN ({search})");
        }

        private async IAsyncEnumerable<ComponentOfferDocument> ListWithQueryAsync(string queryString)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition(queryString);

            var queryIterator = container
                .GetItemQueryIterator<ComponentOfferDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

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

        public async Task<ComponentOfferDocument> RemoveAsync(ComponentOfferDocument componentOffer)
        {
            if (componentOffer is null)
                throw new ArgumentNullException(nameof(componentOffer));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ComponentOfferDocument>(componentOffer.Id, new PartitionKey(Options.TenantName))
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

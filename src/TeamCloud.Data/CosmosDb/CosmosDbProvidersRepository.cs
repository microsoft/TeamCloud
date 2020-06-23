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
using TeamCloud.Model.Internal.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{

    public class CosmosDbProvidersRepository : CosmosDbRepository<Provider>, IProvidersRepository
    {
        public CosmosDbProvidersRepository(ICosmosDbOptions cosmosOptions)
            : base(cosmosOptions)
        { }

        public async Task<Provider> AddAsync(Provider provider)
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

        public async Task<Provider> GetAsync(string id)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<Provider>(id, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<Provider> SetAsync(Provider provider)
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

        public async IAsyncEnumerable<Provider> ListAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");
            var queryIterator = container
                .GetItemQueryIterator<Provider>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

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

        public async IAsyncEnumerable<Provider> ListAsync(IEnumerable<string> ids)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var search = "'" + string.Join(", '", ids) + "'";
            var query = new QueryDefinition($"SELECT * FROM p WHERE p.id IN ({search})");

            var queryIterator = container
                .GetItemQueryIterator<Provider>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

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

        public async Task<Provider> RemoveAsync(Provider provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<Provider>(provider.Id, new PartitionKey(Options.TenantName))
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

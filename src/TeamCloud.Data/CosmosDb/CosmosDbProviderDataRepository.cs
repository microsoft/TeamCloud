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
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbProviderDataRepository : CosmosDbRepository<ProviderDataDocument>, IProviderDataRepository
    {
        public CosmosDbProviderDataRepository(ICosmosDbOptions cosmosOptions)
            : base(cosmosOptions)
        { }

        public async Task<ProviderDataDocument> AddAsync(ProviderDataDocument data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            await data
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);


            var response = await container
                .CreateItemAsync(data, new PartitionKey(Options.TenantName))
                .ConfigureAwait(false);

            return response.Resource;
        }


        public async Task<ProviderDataDocument> GetAsync(string id)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ProviderDataDocument>(id, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }


        public async Task<ProviderDataDocument> GetAsync(string providerId, string nameOrId)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ProviderDataDocument>(nameOrId, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                var query = new QueryDefinition($"SELECT * FROM d WHERE p.providerId = '{providerId}' and p.scope = 'System' and p.name = '{nameOrId}'");

                var queryIterator = container
                    .GetItemQueryIterator<ProviderDataDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

                if (queryIterator.HasMoreResults)
                {
                    var queryResults = await queryIterator
                        .ReadNextAsync()
                        .ConfigureAwait(false);

                    return queryResults.FirstOrDefault();
                }
            }

            return null;
        }

        public async IAsyncEnumerable<ProviderDataDocument> GetByNameAsync(string providerId, string name)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM d WHERE p.providerId = '{providerId}' and p.scope = 'System' and p.name = '{name}'");

            var queryIterator = container
                .GetItemQueryIterator<ProviderDataDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async IAsyncEnumerable<ProviderDataDocument> GetByNameAsync(string providerId, string projectId, string name)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM d WHERE p.providerId = '{providerId}' and p.scope = 'Project' and p.projectId = '{projectId}' and p.name = '{name}'");

            var queryIterator = container
                .GetItemQueryIterator<ProviderDataDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async Task<ProviderDataDocument> SetAsync(ProviderDataDocument data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            await data
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(data, new PartitionKey(Options.TenantName))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public async IAsyncEnumerable<ProviderDataDocument> ListAsync(string providerId, bool includeShared = false)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = includeShared
                ? new QueryDefinition($"SELECT * FROM d WHERE d.providerId = '{providerId}' or d.isShared = true")
                : new QueryDefinition($"SELECT * FROM d WHERE d.providerId = '{providerId}'");

            var queryIterator = container
                .GetItemQueryIterator<ProviderDataDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async IAsyncEnumerable<ProviderDataDocument> ListAsync(string providerId, string projectId, bool includeShared = false)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = includeShared
                ? new QueryDefinition($"SELECT * FROM d WHERE (d.providerId = '{providerId}' or p.isShared = true) and (p.scope = 'System' or (p.scope = 'Project' and p.projectId = '{projectId}'))")
                : new QueryDefinition($"SELECT * FROM d WHERE d.providerId = '{providerId}' and (p.scope = 'System' or (p.scope = 'Project' and p.projectId = '{projectId}'))");

            var queryIterator = container
                .GetItemQueryIterator<ProviderDataDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async Task<ProviderDataDocument> RemoveAsync(ProviderDataDocument data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ProviderDataDocument>(data.Id, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }

        public async Task RemoveAsync(string projectId)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM d WHERE p.scope = 'Project' and p.projectId = '{projectId}'");

            var queryIterator = container
                .GetItemQueryIterator<ProviderDataDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            var deleteTasks = new List<Task>();

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    deleteTasks.Add(container.DeleteItemAsync<ProviderDataDocument>(queryResult.Id, new PartitionKey(Options.TenantName)));
            }

            await Task.WhenAll(deleteTasks)
                .ConfigureAwait(false);
        }
    }
}

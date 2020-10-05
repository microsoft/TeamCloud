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
    public sealed class CosmosDbComponentRepository : CosmosDbRepository<ComponentDocument>, IComponentRepository
    {
        public CosmosDbComponentRepository(ICosmosDbOptions cosmosOptions)
            : base(cosmosOptions)
        { }

        public async Task<ComponentDocument> AddAsync(ComponentDocument component)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            await component
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(component, GetPartitionKey(component))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public async Task<ComponentDocument> GetAsync(string projectId, string id)
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (!Guid.TryParse(projectId, out var projectIdParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (!Guid.TryParse(id, out var idParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ComponentDocument>(idParsed.ToString(), GetPartitionKey(projectIdParsed.ToString()))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async IAsyncEnumerable<ComponentDocument> ListAsync(string projectId, string providerId = null)
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (!Guid.TryParse(projectId, out var projectIdParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var queryString = $"SELECT * FROM c WHERE c.projectId = '{projectIdParsed}'";

            if (!string.IsNullOrEmpty(providerId))
                queryString += $" and c.providerId = '{providerId}'";

            var query = new QueryDefinition(queryString);

            var queryIterator = container
                .GetItemQueryIterator<ComponentDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = GetPartitionKey(projectIdParsed.ToString()) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async Task<ComponentDocument> RemoveAsync(ComponentDocument component)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ComponentDocument>(component.Id, GetPartitionKey(component))
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
            var components = ListAsync(projectId);

            if (await components.AnyAsync().ConfigureAwait(false))
            {
                var container = await GetContainerAsync()
                    .ConfigureAwait(false);

                var batch = container
                    .CreateTransactionalBatch(new PartitionKey(projectId));

                await foreach (var component in components.ConfigureAwait(false))
                    batch = batch.DeleteItem(component.Id);

                await batch
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task RemoveAsync(string projectId, string linkId)
        {
            var component = await GetAsync(projectId, linkId)
                .ConfigureAwait(false);

            if (component != null)
            {
                await RemoveAsync(component)
                    .ConfigureAwait(false);
            }
        }

        public async Task<ComponentDocument> SetAsync(ComponentDocument component)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            await component
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(component, GetPartitionKey(component))
                .ConfigureAwait(false);

            return response.Resource;
        }
    }
}

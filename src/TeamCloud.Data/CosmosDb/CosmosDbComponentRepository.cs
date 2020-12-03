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
    public sealed class CosmosDbComponentRepository : CosmosDbRepository<Component>, IComponentRepository
    {
        public CosmosDbComponentRepository(ICosmosDbOptions cosmosOptions)
            : base(cosmosOptions)
        { }

        public async Task<Component> AddAsync(Component component)
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

        public async Task<Component> GetAsync(string projectId, string id)
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (!Guid.TryParse(id, out var idParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(id));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<Component>(idParsed.ToString(), GetPartitionKey(projectId))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async IAsyncEnumerable<Component> ListAsync(string projectId)
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (!Guid.TryParse(projectId, out var projectIdParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var queryString = $"SELECT * FROM c WHERE c.projectId = '{projectIdParsed}'";

            // if (!string.IsNullOrEmpty(providerId))
            //     queryString += $" and c.providerId = '{providerId}'";

            var query = new QueryDefinition(queryString);

            var queryIterator = container
                .GetItemQueryIterator<Component>(query, requestOptions: GetQueryRequestOptions(projectId));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async Task<Component> RemoveAsync(Component component)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<Component>(component.Id, GetPartitionKey(component))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }

        public async Task RemoveAllAsync(string projectId)
        {
            var components = ListAsync(projectId);

            if (await components.AnyAsync().ConfigureAwait(false))
            {
                var container = await GetContainerAsync()
                    .ConfigureAwait(false);

                var batch = container
                    .CreateTransactionalBatch(GetPartitionKey(projectId));

                await foreach (var component in components.ConfigureAwait(false))
                    batch = batch.DeleteItem(component.Id);

                await batch
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task RemoveAsync(string projectId, string id)
        {
            var component = await GetAsync(projectId, id)
                .ConfigureAwait(false);

            if (component != null)
            {
                await RemoveAsync(component)
                    .ConfigureAwait(false);
            }
        }

        public async Task<Component> SetAsync(Component component)
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

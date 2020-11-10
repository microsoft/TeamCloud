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
    public class CosmosDbComponentTemplateRepository : CosmosDbRepository<ComponentTemplate>, IComponentTemplateRepository
    {
        public CosmosDbComponentTemplateRepository(ICosmosDbOptions cosmosOptions)
            : base(cosmosOptions)
        { }

        public async Task<ComponentTemplate> AddAsync(ComponentTemplate componentTemplate)
        {
            if (componentTemplate is null)
                throw new ArgumentNullException(nameof(componentTemplate));

            await componentTemplate
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(componentTemplate, new PartitionKey(componentTemplate.Organization))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public async Task<ComponentTemplate> GetAsync(string organization, string id)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ComponentTemplate>(id, new PartitionKey(organization))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<ComponentTemplate> SetAsync(ComponentTemplate componentTemplate)
        {
            if (componentTemplate is null)
                throw new ArgumentNullException(nameof(componentTemplate));

            await componentTemplate
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(componentTemplate, new PartitionKey(componentTemplate.Organization))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public IAsyncEnumerable<ComponentTemplate> ListAsync(string organization)
            => ListWithQueryAsync(organization, $"SELECT * FROM o");

        public IAsyncEnumerable<ComponentTemplate> ListAsync(string organization, string parentId)
            => ListWithQueryAsync(organization, $"SELECT * FROM o WHERE o.parentId = '{parentId}'");

        public IAsyncEnumerable<ComponentTemplate> ListAsync(string organization, IEnumerable<string> parentIds)
        {
            var search = "'" + string.Join("', '", parentIds) + "'";
            return ListWithQueryAsync(organization, $"SELECT * FROM o WHERE o.parentId IN ({search})");
        }

        private async IAsyncEnumerable<ComponentTemplate> ListWithQueryAsync(string organization, string queryString)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition(queryString);

            var queryIterator = container
                .GetItemQueryIterator<ComponentTemplate>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(organization) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async Task<ComponentTemplate> RemoveAsync(ComponentTemplate componentTemplate)
        {
            if (componentTemplate is null)
                throw new ArgumentNullException(nameof(componentTemplate));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ComponentTemplate>(componentTemplate.Id, new PartitionKey(componentTemplate.Organization))
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

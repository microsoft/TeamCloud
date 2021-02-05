/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.Cosmos;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public sealed class CosmosDbComponentTaskRepository : CosmosDbRepository<ComponentTask>, IComponentTaskRepository
    {
        public CosmosDbComponentTaskRepository(ICosmosDbOptions options, IEnumerable<IDocumentExpander> expanders, IDataProtectionProvider dataProtectionProvider = null)
            : base(options, expanders, dataProtectionProvider)
        { }

        public override async Task<ComponentTask> AddAsync(ComponentTask task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            await task
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(task, GetPartitionKey(task))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public override async Task<ComponentTask> GetAsync(string componentId, string id, bool expand = false)
        {
            if (componentId is null)
                throw new ArgumentNullException(nameof(componentId));

            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (!Guid.TryParse(id, out var idParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(id));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ComponentTask>(idParsed.ToString(), GetPartitionKey(componentId))
                    .ConfigureAwait(false);

                var expandTask = expand
                    ? ExpandAsync(response.Resource)
                    : Task.FromResult(response.Resource);

                return await expandTask.ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public override async IAsyncEnumerable<ComponentTask> ListAsync(string componentId)
        {
            if (componentId is null)
                throw new ArgumentNullException(nameof(componentId));

            if (!Guid.TryParse(componentId, out var componentIdParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(componentId));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var queryString = $"SELECT * FROM c WHERE c.componentId = '{componentIdParsed}'";

            var query = new QueryDefinition(queryString);

            var queryIterator = container
                .GetItemQueryIterator<ComponentTask>(query, requestOptions: GetQueryRequestOptions(componentId));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async Task RemoveAllAsync(string componentId)
        {
            var components = ListAsync(componentId);

            if (await components.AnyAsync().ConfigureAwait(false))
            {
                var container = await GetContainerAsync()
                    .ConfigureAwait(false);

                var batch = container
                    .CreateTransactionalBatch(GetPartitionKey(componentId));

                await foreach (var component in components.ConfigureAwait(false))
                    batch = batch.DeleteItem(component.Id);

                await batch
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
        }

        public override async Task<ComponentTask> RemoveAsync(ComponentTask task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ComponentTask>(task.Id, GetPartitionKey(task))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }

        public async Task RemoveAsync(string componentId, string id)
        {
            var component = await GetAsync(componentId, id)
                .ConfigureAwait(false);

            if (component != null)
            {
                await RemoveAsync(component)
                    .ConfigureAwait(false);
            }
        }

        public override async Task<ComponentTask> SetAsync(ComponentTask task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            await task
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(task, GetPartitionKey(task))
                .ConfigureAwait(false);

            return response.Resource;
        }
    }
}

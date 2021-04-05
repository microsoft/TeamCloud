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
    public sealed class CosmosDbComponentRepository : CosmosDbRepository<Component>, IComponentRepository
    {
        public CosmosDbComponentRepository(ICosmosDbOptions options, IDocumentExpanderProvider expanderProvider = null, IDocumentSubscriptionProvider subscriptionProvider = null, IDataProtectionProvider dataProtectionProvider = null)
            : base(options, expanderProvider, subscriptionProvider, dataProtectionProvider)
        { }

        public override async Task<Component> AddAsync(Component component)
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

            return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Create)
                .ConfigureAwait(false);
        }

        public override async Task<Component> GetAsync(string projectId, string id, bool expand = false)
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (id is null)
                throw new ArgumentNullException(nameof(id));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<Component>(id, GetPartitionKey(projectId))
                    .ConfigureAwait(false);

                var expandTask = expand
                    ? ExpandAsync(response.Resource)
                    : Task.FromResult(response.Resource);

                return await expandTask.ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                var query = new QueryDefinition($"SELECT * FROM o WHERE o.slug = '{id}'");

                var queryIterator = container
                    .GetItemQueryIterator<Component>(query, requestOptions: GetQueryRequestOptions(projectId));

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

        public override IAsyncEnumerable<Component> ListAsync(string projectId)
            => ListAsync(projectId, includeDeleted: false);

        public async IAsyncEnumerable<Component> ListAsync(string projectId, bool includeDeleted)
        {
            if (projectId is null)
                throw new ArgumentNullException(nameof(projectId));

            if (!Guid.TryParse(projectId, out var projectIdParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var queryString = $"SELECT * FROM c WHERE c.projectId = '{projectIdParsed}'";

            if (!includeDeleted)
                queryString += " AND NOT IS_DEFINED(c.deleted)";

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

        public override Task<Component> RemoveAsync(Component component)
            => RemoveAsync(component, soft: true);

        public async Task<Component> RemoveAsync(Component component, bool soft)
        {
            if (component is null)
                throw new ArgumentNullException(nameof(component));

            if (soft && component.Deleted.HasValue && component.TTL.HasValue)
                return component;

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                if (soft)
                {
                    component.Deleted = DateTime.UtcNow;
                    component.TTL = GetSoftDeleteTTL();

                    var response = await container
                        .UpsertItemAsync(component, GetPartitionKey(component))
                        .ConfigureAwait(false);

                    return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Delete)
                        .ConfigureAwait(false);
                }
                else
                {
                    var response = await container
                        .DeleteItemAsync<Component>(component.Id, GetPartitionKey(component))
                        .ConfigureAwait(false);

                    return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Delete)
                        .ConfigureAwait(false);
                }
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }

        public async Task RemoveAllAsync(string projectId, bool soft)
        {
            var components = ListAsync(projectId, includeDeleted: !soft);

            if (await components.AnyAsync().ConfigureAwait(false))
            {
                var container = await GetContainerAsync()
                    .ConfigureAwait(false);

                var batch = container
                    .CreateTransactionalBatch(GetPartitionKey(projectId));

                if (soft)
                {
                    var now = DateTime.UtcNow;
                    var ttl = GetSoftDeleteTTL();

                    await foreach (var component in components.ConfigureAwait(false))
                    {
                        component.Deleted = now;
                        component.TTL = ttl;
                        batch = batch.UpsertItem(component);
                    }
                }
                else
                {
                    await foreach (var component in components.ConfigureAwait(false))
                        batch = batch.DeleteItem(component.Id);
                }

                using var batchResult = await batch
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                if (batchResult.IsSuccessStatusCode)
                {
                    _ = await NotifySubscribersAsync(batchResult.GetOperationResultResources<Component>(), DocumentSubscriptionEvent.Delete).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception(batchResult.ErrorMessage);
                }
            }
        }

        public override async Task<Component> SetAsync(Component component)
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

            return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Update)
                .ConfigureAwait(false);
        }
    }
}

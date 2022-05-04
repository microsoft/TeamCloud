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
using Microsoft.Extensions.Caching.Memory;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Validation;
using TeamCloud.Validation.Providers;

namespace TeamCloud.Data.CosmosDb;

public sealed class CosmosDbComponentTaskRepository : CosmosDbRepository<ComponentTask>, IComponentTaskRepository
{
    public CosmosDbComponentTaskRepository(ICosmosDbOptions options, IMemoryCache cache, IValidatorProvider validatorProvider, IDocumentExpanderProvider expanderProvider = null, IDocumentSubscriptionProvider subscriptionProvider = null, IDataProtectionProvider dataProtectionProvider = null)
        : base(options, validatorProvider, expanderProvider, subscriptionProvider, dataProtectionProvider, cache)
    { }

    public override async Task<ComponentTask> AddAsync(ComponentTask task)
    {
        if (task is null)
            throw new ArgumentNullException(nameof(task));

        await task
            .ValidateAsync(ValidatorProvider, throwOnValidationError: true)
            .ConfigureAwait(false);

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var response = await container
            .CreateItemAsync(task, GetPartitionKey(task))
            .ConfigureAwait(false);

        return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Create)
            .ConfigureAwait(false);
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

        ComponentTask componentTask = null;

        try
        {
            var response = await container
                .ReadItemAsync<ComponentTask>(idParsed.ToString(), GetPartitionKey(componentId))
                .ConfigureAwait(false);

            componentTask = response.Resource;
        }
        catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        return await ExpandAsync(componentTask, expand)
            .ConfigureAwait(false);
    }

    public override async IAsyncEnumerable<ComponentTask> ListAsync(string componentId)
    {
        if (componentId is null)
            throw new ArgumentNullException(nameof(componentId));

        if (!Guid.TryParse(componentId, out var componentIdParsed))
            throw new ArgumentException("Value is not a valid GUID", nameof(componentId));

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var query = new QueryDefinition("SELECT * FROM c WHERE c.componentId = @componentId")
            .WithParameter("@componentId", componentIdParsed.ToString());

        var componentTasks = container
            .GetItemQueryIterator<ComponentTask>(query, requestOptions: GetQueryRequestOptions(componentId))
            .ReadAllAsync(item => ExpandAsync(item))
            .ConfigureAwait(false);

        await foreach (var componentTask in componentTasks)
            yield return componentTask;
    }

    public async Task RemoveAllAsync(string componentId)
    {
        var componentTasks = ListAsync(componentId);

        if (await componentTasks.AnyAsync().ConfigureAwait(false))
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var batch = container
                .CreateTransactionalBatch(GetPartitionKey(componentId));

            await foreach (var componentTask in componentTasks.ConfigureAwait(false))
                batch = batch.DeleteItem(componentTask.Id);

            var batchResponse = await batch
                .ExecuteAsync()
                .ConfigureAwait(false);

            if (batchResponse.IsSuccessStatusCode)
            {
                _ = await NotifySubscribersAsync(batchResponse.GetOperationResultResources<ComponentTask>(), DocumentSubscriptionEvent.Delete).ConfigureAwait(false);
            }
            else
            {
                throw new Exception(batchResponse.ErrorMessage);
            }
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

            return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Delete)
                .ConfigureAwait(false);
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

        if (component is not null)
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
            .ValidateAsync(ValidatorProvider, throwOnValidationError: true)
            .ConfigureAwait(false);

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var response = await container
            .UpsertItemAsync(task, GetPartitionKey(task))
            .ConfigureAwait(false);

        return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Update)
            .ConfigureAwait(false);
    }
}

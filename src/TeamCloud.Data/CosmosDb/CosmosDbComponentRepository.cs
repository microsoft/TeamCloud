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

public sealed class CosmosDbComponentRepository : CosmosDbRepository<Component>, IComponentRepository
{
    private readonly IComponentTaskRepository componentTaskRepository;

    public CosmosDbComponentRepository(ICosmosDbOptions options, IMemoryCache cache, IComponentTaskRepository componentTaskRepository, IValidatorProvider validatorProvider, IDocumentExpanderProvider expanderProvider = null, IDocumentSubscriptionProvider subscriptionProvider = null, IDataProtectionProvider dataProtectionProvider = null)
        : base(options, validatorProvider, expanderProvider, subscriptionProvider, dataProtectionProvider, cache)
    {
        this.componentTaskRepository = componentTaskRepository ?? throw new ArgumentNullException(nameof(componentTaskRepository));
    }

    public override async Task<Component> AddAsync(Component component)
    {
        if (component is null)
            throw new ArgumentNullException(nameof(component));

        await component
            .ValidateAsync(ValidatorProvider, throwOnValidationError: true)
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

        Component component = null;

        try
        {
            var response = await container
                .ReadItemAsync<Component>(id, GetPartitionKey(projectId))
                .ConfigureAwait(false);

            component = response.Resource;
        }
        catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
        {
            var query = new QueryDefinition($"SELECT * FROM o WHERE o.slug = @identifier OFFSET 0 LIMIT 1")
                .WithParameter("@identifier", id.ToLowerInvariant());

            var queryIterator = container
                .GetItemQueryIterator<Component>(query, requestOptions: GetQueryRequestOptions(projectId));

            if (queryIterator.HasMoreResults)
            {
                var queryResults = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                component = queryResults.FirstOrDefault();
            }
        }

        return await ExpandAsync(component, expand)
                .ConfigureAwait(false);
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

        var queryString = $"SELECT * FROM c WHERE c.projectId = @projectId";

        if (!includeDeleted)
            queryString += " AND NOT IS_DEFINED(c.deleted)";

        var query = new QueryDefinition(queryString)
            .WithParameter("@projectId", projectIdParsed.ToString());

        var components = container
            .GetItemQueryIterator<Component>(query)
            .ReadAllAsync(item => ExpandAsync(item))
            .ConfigureAwait(false);

        await foreach (var component in components)
            yield return component;
    }


    public IAsyncEnumerable<Component> ListAsync(string projectId, IEnumerable<string> identifiers)
        => ListAsync(projectId, identifiers, includeDeleted: false);

    public async IAsyncEnumerable<Component> ListAsync(string projectId, IEnumerable<string> identifiers, bool includeDeleted)
    {
        if (projectId is null)
            throw new ArgumentNullException(nameof(projectId));

        if (!Guid.TryParse(projectId, out var projectIdParsed))
            throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var search = "'" + string.Join("', '", identifiers) + "'";
        var searchLower = "'" + string.Join("', '", identifiers.Select(i => i.ToLowerInvariant())) + "'";

        var queryString = $"SELECT * FROM c WHERE c.projectId = '{projectIdParsed}' AND (c.id IN ({search}) OR c.slug IN ({searchLower}))";

        if (!includeDeleted)
            queryString += " AND NOT IS_DEFINED(c.deleted)";

        var query = new QueryDefinition(queryString);

        var components = container
            .GetItemQueryIterator<Component>(query)
            .ReadAllAsync(item => ExpandAsync(item))
            .ConfigureAwait(false);

        await foreach (var component in components)
            yield return component;
    }

    public override Task<Component> RemoveAsync(Component component)
        => RemoveAsync(component, soft: true);

    public async Task<Component> RemoveAsync(Component component, bool soft)
    {
        if (component is null)
            throw new ArgumentNullException(nameof(component));

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        try
        {
            if (soft)
            {
                component.Deleted ??= DateTime.UtcNow;
                component.TTL ??= GetSoftDeleteTTL();

                return await SetAsync(component)
                    .ConfigureAwait(false);
            }
            else
            {
                await componentTaskRepository
                    .RemoveAllAsync(component.Id)
                    .ConfigureAwait(false);

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

    public IAsyncEnumerable<Component> ListByDeploymentScopeAsync(string deploymentScopeId)
        => ListByDeploymentScopeAsync(deploymentScopeId, includeDeleted: false);

    public async IAsyncEnumerable<Component> ListByDeploymentScopeAsync(string deploymentScopeId, bool includeDeleted)
    {
        if (deploymentScopeId is null)
            throw new ArgumentNullException(nameof(deploymentScopeId));

        if (!Guid.TryParse(deploymentScopeId, out var deploymentScopeIdParsed))
            throw new ArgumentException("Value is not a valid GUID", nameof(deploymentScopeId));

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var queryString = $"SELECT * FROM c WHERE c.deploymentScopeId = @deploymentScopeId";

        if (!includeDeleted)
            queryString += " AND NOT IS_DEFINED(c.deleted)";

        var query = new QueryDefinition(queryString)
            .WithParameter("@deploymentScopeId", deploymentScopeIdParsed.ToString());

        var components = container
            .GetItemQueryIterator<Component>(query)
            .ReadAllAsync(item => ExpandAsync(item))
            .ConfigureAwait(false);

        await foreach (var component in components)
            yield return component;

    }

    public override async Task<Component> SetAsync(Component component)
    {
        if (component is null)
            throw new ArgumentNullException(nameof(component));

        await component
            .ValidateAsync(ValidatorProvider, throwOnValidationError: true)
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

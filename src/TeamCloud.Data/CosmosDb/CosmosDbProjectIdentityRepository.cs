/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
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

public sealed class CosmosDbProjectIdentityRepository : CosmosDbRepository<ProjectIdentity>, IProjectIdentityRepository
{
    public CosmosDbProjectIdentityRepository(ICosmosDbOptions options, IMemoryCache cache, IValidatorProvider validatorProvider = null, IDocumentExpanderProvider expanderProvider = null, IDocumentSubscriptionProvider subscriptionProvider = null, IDataProtectionProvider dataProtectionProvider = null)
        : base(options, validatorProvider, expanderProvider, subscriptionProvider, dataProtectionProvider, cache)
    { }

    public override async Task<ProjectIdentity> AddAsync(ProjectIdentity document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        _ = await document
            .ValidateAsync(ValidatorProvider, throwOnValidationError: true)
            .ConfigureAwait(false);

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        try
        {
            var response = await container
                .CreateItemAsync(document)
                .ConfigureAwait(false);

            return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Create)
                .ConfigureAwait(false);
        }
        catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
        {
            throw; // Indicates a name conflict (already a project with name)
        }
    }

    public override Task<ProjectIdentity> GetAsync(string projectId, string identifier, bool expand = false) => GetCachedAsync(projectId, identifier, async cached =>
    {
        if (projectId is null)
            throw new ArgumentNullException(nameof(projectId));

        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        if (!Guid.TryParse(identifier, out var identifierParsed))
            throw new ArgumentException("Value is not a valid GUID", nameof(identifier));

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        ProjectIdentity projectIdentity = null;

        try
        {
            var response = await container
                .ReadItemAsync<ProjectIdentity>(identifierParsed.ToString(), GetPartitionKey(projectId))
                .ConfigureAwait(false);

            projectIdentity = SetCached(projectId, identifier, response.Resource);
        }
        catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotModified)
        {
            projectIdentity = cached;
        }
        catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        return await ExpandAsync(projectIdentity, expand)
            .ConfigureAwait(false);
    });

    public override async IAsyncEnumerable<ProjectIdentity> ListAsync(string projectId)
    {
        if (projectId is null)
            throw new ArgumentNullException(nameof(projectId));

        if (!Guid.TryParse(projectId, out var projectIdParsed))
            throw new ArgumentException("Value is not a valid GUID", nameof(projectId));

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var query = new QueryDefinition("SELECT * FROM c WHERE c.projectId = @identifier")
            .WithParameter("@identifier", projectIdParsed.ToString());

        var projectIdentities = container
            .GetItemQueryIterator<ProjectIdentity>(query, requestOptions: GetQueryRequestOptions(projectId))
            .ReadAllAsync(item => ExpandAsync(item))
            .ConfigureAwait(false);

        await foreach (var projectIdentity in projectIdentities)
            yield return projectIdentity;
    }

    public override async Task<ProjectIdentity> RemoveAsync(ProjectIdentity document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        try
        {
            var response = await container
                .DeleteItemAsync<ProjectIdentity>(document.Id, GetPartitionKey(document))
                .ConfigureAwait(false);

            return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Delete)
                .ConfigureAwait(false);
        }
        catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
        {
            return null; // already deleted
        }
    }

    public override async Task<ProjectIdentity> SetAsync(ProjectIdentity document)
    {
        if (document is null)
            throw new ArgumentNullException(nameof(document));

        await document
            .ValidateAsync(ValidatorProvider, throwOnValidationError: true)
            .ConfigureAwait(false);

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var response = await container
            .UpsertItemAsync(document, GetPartitionKey(document))
            .ConfigureAwait(false);

        return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Update)
            .ConfigureAwait(false);
    }
}

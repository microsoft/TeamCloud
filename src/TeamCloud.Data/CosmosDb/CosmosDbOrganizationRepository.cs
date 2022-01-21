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

public class CosmosDbOrganizationRepository : CosmosDbRepository<Organization>, IOrganizationRepository
{
    private readonly IMemoryCache cache;

    public CosmosDbOrganizationRepository(ICosmosDbOptions options, IMemoryCache cache, IValidatorProvider validatorProvider, IDocumentExpanderProvider expanderProvider = null, IDocumentSubscriptionProvider subscriptionProvider = null, IDataProtectionProvider dataProtectionProvider = null)
        : base(options, validatorProvider, expanderProvider, subscriptionProvider, dataProtectionProvider, cache)
    {
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }


    public async Task<string> ResolveIdAsync(string tenant, string identifier)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        var key = $"{tenant}_{identifier}";

        if (!cache.TryGetValue(key, out string id))
        {
            var organization = await GetAsync(tenant, identifier)
                .ConfigureAwait(false);

            id = organization?.Id;

            if (!string.IsNullOrEmpty(id))
                cache.Set(key, cache, TimeSpan.FromMinutes(10));
        }

        return id;
    }

    public override async Task<Organization> AddAsync(Organization organization)
    {
        if (organization is null)
            throw new ArgumentNullException(nameof(organization));

        await organization
            .ValidateAsync(ValidatorProvider, throwOnValidationError: true)
            .ConfigureAwait(false);

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var response = await container
            .CreateItemAsync(organization, GetPartitionKey(organization))
            .ConfigureAwait(false);

        return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Create)
            .ConfigureAwait(false);
    }

    public override Task<Organization> GetAsync(string tenant, string identifier, bool expand = false) => GetCachedAsync(tenant, identifier, (async (cached) =>
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        Organization organization = null;

        try
        {
            var response = await container
                .ReadItemAsync<Organization>(identifier, GetPartitionKey(tenant), cached?.GetItemNoneMatchRequestOptions())
                .ConfigureAwait(false);

            organization = SetCached(tenant, identifier, response.Resource);
        }
        catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotModified)
        {
            organization = cached;
        }
        catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
        {
            var query = new QueryDefinition($"SELECT * FROM o WHERE o.slug = @identifier OFFSET 0 LIMIT 1")
                .WithParameter("@identifier", identifier.ToLowerInvariant());

            var queryIterator = container
                .GetItemQueryIterator<Organization>(query, requestOptions: GetQueryRequestOptions(tenant));

            if (queryIterator.HasMoreResults)
            {
                var queryResults = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                organization = queryResults.FirstOrDefault();
            }
        }

        return await ExpandAsync(organization, expand)
            .ConfigureAwait(false);
    }));

    public override async IAsyncEnumerable<Organization> ListAsync(string tenant)
    {
        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var query = new QueryDefinition($"SELECT * FROM o");

        var organizations = container
            .GetItemQueryIterator<Organization>(query, requestOptions: GetQueryRequestOptions(tenant))
            .ReadAllAsync(item => ExpandAsync(item))
            .ConfigureAwait(false);

        await foreach (var organization in organizations)
            yield return organization;
    }

    public async IAsyncEnumerable<Organization> ListAsync(string tenant, IEnumerable<string> identifiers)
    {
        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var search = "'" + string.Join("', '", identifiers) + "'";
        var searchLower = "'" + string.Join("', '", identifiers.Select(i => i.ToLowerInvariant())) + "'";
        var query = new QueryDefinition($"SELECT * FROM o WHERE o.id IN ({search}) OR o.slug IN ({searchLower}) OR LOWER(o.displayName) in ({searchLower})");

        var organizations = container
            .GetItemQueryIterator<Organization>(query, requestOptions: GetQueryRequestOptions(tenant))
            .ReadAllAsync(item => ExpandAsync(item))
            .ConfigureAwait(false);

        await foreach (var organization in organizations)
            yield return organization;
    }

    public override async Task<Organization> RemoveAsync(Organization organization)
    {
        if (organization is null)
            throw new ArgumentNullException(nameof(organization));

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        try
        {
            var response = await container
                .DeleteItemAsync<Organization>(organization.Id, GetPartitionKey(organization))
                .ConfigureAwait(false);

            return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Delete)
                .ConfigureAwait(false);
        }
        catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
        {
            return null; // already deleted
        }
    }

    public override async Task<Organization> SetAsync(Organization organization)
    {
        if (organization is null)
            throw new ArgumentNullException(nameof(organization));

        await organization
            .ValidateAsync(ValidatorProvider, throwOnValidationError: true)
            .ConfigureAwait(false);

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var response = await container
            .UpsertItemAsync(organization, GetPartitionKey(organization))
            .ConfigureAwait(false);

        return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Update)
            .ConfigureAwait(false);
    }
}

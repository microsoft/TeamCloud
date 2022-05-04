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
using TeamCloud.Serialization;

namespace TeamCloud.Data.CosmosDb;

public class CosmosDbProjectRepository : CosmosDbRepository<Project>, IProjectRepository
{
    private readonly IMemoryCache cache;

    private readonly IUserRepository userRepository;

    public CosmosDbProjectRepository(ICosmosDbOptions options, IUserRepository userRepository, IMemoryCache cache, IValidatorProvider validatorProvider, IDocumentExpanderProvider expanderProvider = null, IDocumentSubscriptionProvider subscriptionProvider = null, IDataProtectionProvider dataProtectionProvider = null)
        : base(options, validatorProvider, expanderProvider, subscriptionProvider, dataProtectionProvider, cache)
    {
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<string> ResolveIdAsync(string organization, string identifier)
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        var key = $"{organization}_{identifier}";

        if (!cache.TryGetValue(key, out string id))
        {
            var project = await GetAsync(organization, identifier)
                .ConfigureAwait(false);

            id = project?.Id;

            if (!string.IsNullOrEmpty(id))
                cache.Set(key, cache, TimeSpan.FromMinutes(10));
        }

        return id;
    }

    private void RemoveCachedIds(Project project)
    {
        cache.Remove($"{project.Organization}_{project.DisplayName}");
        cache.Remove($"{project.Organization}_{project.Slug}");
        cache.Remove($"{project.Organization}_{project.Id}");
    }

    public override async Task<Project> AddAsync(Project project)
    {
        if (project is null)
            throw new ArgumentNullException(nameof(project));

        await project
            .ValidateAsync(ValidatorProvider, throwOnValidationError: true)
            .ConfigureAwait(false);

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        try
        {
            var response = await container
                .CreateItemAsync(project)
                .ConfigureAwait(false);

            project = await ExpandAsync(response.Resource)
                .ConfigureAwait(false);

            return await NotifySubscribersAsync(project, DocumentSubscriptionEvent.Create)
                .ConfigureAwait(false);
        }
        catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
        {
            throw; // Indicates a name conflict (already a project with name)
        }
    }

    public override async Task<Project> GetAsync(string organization, string identifier, bool expand = false) 
    {
        if (identifier is null)
            throw new ArgumentNullException(nameof(identifier));

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        Project project = null;

        try
        {
            var response = await container
                .ReadItemAsync<Project>(identifier, GetPartitionKey(organization))
                .ConfigureAwait(false);

            project = response.Resource;
        }
        catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
        {
            var query = new QueryDefinition($"SELECT * FROM c WHERE  c.slug = @identifier OFFSET 0 LIMIT 1")
                .WithParameter("@identifier", identifier.ToLowerInvariant());

            var queryIterator = container
                .GetItemQueryIterator<Project>(query, requestOptions: GetQueryRequestOptions(organization));

            if (queryIterator.HasMoreResults)
            {
                var queryResults = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                project = queryResults.FirstOrDefault();
            }
        }

        return await ExpandAsync(project, expand)
            .ConfigureAwait(false);
    }

    public async Task<bool> NameExistsAsync(string organization, string name)
    {
        var project = await ResolveIdAsync(organization, name)
            .ConfigureAwait(false);

        return project is not null;
    }

    public override async Task<Project> SetAsync(Project project)
    {
        if (project is null)
            throw new ArgumentNullException(nameof(project));

        await project
            .ValidateAsync(ValidatorProvider, throwOnValidationError: true)
            .ConfigureAwait(false);

        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var response = await container
            .UpsertItemAsync(project, GetPartitionKey(project))
            .ConfigureAwait(false);

        project = await ExpandAsync(response.Resource)
            .ConfigureAwait(false);

        return await NotifySubscribersAsync(project, DocumentSubscriptionEvent.Update)
            .ConfigureAwait(false);
    }

    public override async IAsyncEnumerable<Project> ListAsync(string organization)
    {
        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var query = new QueryDefinition($"SELECT * FROM p");

        var projects = container
            .GetItemQueryIterator<Project>(query, requestOptions: GetQueryRequestOptions(organization))
            .ReadAllAsync(item => ExpandAsync(item))
            .ConfigureAwait(false);

        await foreach (var project in projects)
            yield return project;
    }

    public async IAsyncEnumerable<Project> ListAsync(string organization, IEnumerable<string> identifiers)
    {
        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var query = new QueryDefinition($"SELECT * FROM p WHERE ARRAY_CONTAINS(@ids, p.id) OR ARRAY_CONTAINS(@slugs, p.slug)")
            .WithParameter("@ids", TeamCloudSerialize.SerializeObject(identifiers.ToArray()))
            .WithParameter("@slugs", TeamCloudSerialize.SerializeObject(identifiers.Select(item => item?.ToLowerInvariant()).ToArray()));

        var projects = container
            .GetItemQueryIterator<Project>(query, requestOptions: GetQueryRequestOptions(organization))
            .ReadAllAsync(item => ExpandAsync(item))
            .ConfigureAwait(false);

        await foreach (var project in projects)
            yield return project;
    }


    public async IAsyncEnumerable<Project> ListByTemplateAsync(string organization, string template)
    {
        var container = await GetContainerAsync()
            .ConfigureAwait(false);

        var query = new QueryDefinition("SELECT VALUE p FROM p WHERE p.template = @template")
            .WithParameter("@template", template);

        var projects = container
            .GetItemQueryIterator<Project>(query, requestOptions: GetQueryRequestOptions(organization))
            .ReadAllAsync(item => ExpandAsync(item))
            .ConfigureAwait(false);

        await foreach (var project in projects)
            yield return project;
    }

    public override Task<Project> RemoveAsync(Project project)
        => RemoveAsync(project, soft: true);

    public async Task<Project> RemoveAsync(Project project, bool soft)
    {
        if (project is null)
            throw new ArgumentNullException(nameof(project));

        if (soft)
        {
            project.Deleted ??= DateTime.UtcNow;
            project.TTL = GetSoftDeleteTTL();

            return await SetAsync(project).ConfigureAwait(false);
        }
        else
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<Project>(project.Id, GetPartitionKey(project))
                    .ConfigureAwait(false);

                RemoveCachedIds(project);

                await userRepository
                    .RemoveProjectMembershipsAsync(project.Organization, project.Id)
                    .ConfigureAwait(false);

                return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Delete)
                    .ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }
    }
}

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
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbProjectRepository : CosmosDbRepository<Project>, IProjectRepository
    {
        private readonly IMemoryCache cache;

        private readonly IUserRepository userRepository;

        public CosmosDbProjectRepository(ICosmosDbOptions options, IUserRepository userRepository, IMemoryCache cache, IDocumentExpanderProvider expanderProvider, IDataProtectionProvider dataProtectionProvider = null)
            : base(options, expanderProvider, dataProtectionProvider)
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
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .CreateItemAsync(project)
                    .ConfigureAwait(false);

                await PopulateUsersAsync(response.Resource)
                    .ConfigureAwait(false);

                return response.Resource;
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
                var identifierLower = identifier.ToLowerInvariant();

                var query = new QueryDefinition($"SELECT * FROM c WHERE  c.slug = '{identifierLower}' OR LOWER(c.displayName) = '{identifierLower}'");

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

            await PopulateUsersAsync(project)
                .ConfigureAwait(false);

            if (expand)
            {
                project = await ExpandAsync(project)
                    .ConfigureAwait(false);
            }

            return project;
        }

        public async Task<bool> NameExistsAsync(string organization, string name)
        {
            var project = await ResolveIdAsync(organization, name)
                .ConfigureAwait(false);

            return project != null;
        }

        public override async Task<Project> SetAsync(Project project)
        {
            if (project is null)
                throw new ArgumentNullException(nameof(project));

            await project
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(project, GetPartitionKey(project))
                .ConfigureAwait(false);

            await PopulateUsersAsync(response.Resource)
                .ConfigureAwait(false);

            return response.Resource;
        }

        public override async IAsyncEnumerable<Project> ListAsync(string organization)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM p");

            var queryIterator = container
                .GetItemQueryIterator<Project>(query, requestOptions: GetQueryRequestOptions(organization));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var project in queryResponse)
                {
                    await PopulateUsersAsync(project)
                        .ConfigureAwait(false);

                    yield return project;
                }
            }
        }

        public async IAsyncEnumerable<Project> ListAsync(string organization, IEnumerable<string> identifiers)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var search = "'" + string.Join("', '", identifiers) + "'";
            var searchLower = "'" + string.Join("', '", identifiers.Select(i => i.ToLowerInvariant())) + "'";
            var query = new QueryDefinition($"SELECT * FROM p WHERE p.id IN ({search}) OR p.slug IN ({searchLower}) OR LOWER(p.displayName) in ({searchLower})");

            var queryIterator = container
                .GetItemQueryIterator<Project>(query, requestOptions: GetQueryRequestOptions(organization));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var project in queryResponse)
                {
                    await PopulateUsersAsync(project)
                        .ConfigureAwait(false);

                    yield return project;
                }
            }
        }


        public async IAsyncEnumerable<Project> ListByTemplateAsync(string organization, string template)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT VALUE p FROM p WHERE p.template = '{template}'");

            var queryIterator = container
                .GetItemQueryIterator<Project>(query, requestOptions: GetQueryRequestOptions(organization));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var project in queryResponse)
                {
                    await PopulateUsersAsync(project)
                        .ConfigureAwait(false);

                    yield return project;
                }
            }
        }

        public override async Task<Project> RemoveAsync(Project project)
        {
            if (project is null)
                throw new ArgumentNullException(nameof(project));

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

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }

        private async Task<Project> PopulateUsersAsync(Project project)
        {
            if (project != null)
                project.Users = await userRepository
                    .ListAsync(project.Organization, project.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);

            return project;
        }
    }
}

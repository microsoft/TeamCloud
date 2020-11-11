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
    public class CosmosDbProjectRepository : CosmosDbRepository<Project>, IProjectRepository
    {
        private readonly IUserRepository userRepository;

        public CosmosDbProjectRepository(ICosmosDbOptions cosmosOptions, IUserRepository userRepository)
            : base(cosmosOptions)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<Project> AddAsync(Project project)
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

        public async Task<Project> GetAsync(string organization, string nameOrId)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            Project project = null;

            try
            {
                var response = await container
                    .ReadItemAsync<Project>(nameOrId, GetPartitionKey(organization))
                    .ConfigureAwait(false);

                project = response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                var query = new QueryDefinition($"SELECT * FROM c WHERE c.displayName = '{nameOrId}' OR c.slug = '{nameOrId}'");

                var queryIterator = container
                    .GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = GetPartitionKey(organization) });

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

            return project;
        }

        public async Task<bool> NameExistsAsync(string organization, string name)
        {
            var project = await GetAsync(organization, name)
                .ConfigureAwait(false);

            return project != null;
        }

        public async Task<Project> SetAsync(Project project)
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

        public async IAsyncEnumerable<Project> ListAsync(string organization)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM p");

            var queryIterator = container
                .GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = GetPartitionKey(organization) });

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

        public async IAsyncEnumerable<Project> ListAsync(string organization, IEnumerable<string> nameOrIds)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var search = "'" + string.Join("', '", nameOrIds) + "'";
            var query = new QueryDefinition($"SELECT * FROM p WHERE p.id IN ({search}) OR p.slug IN ({search}) OR p.displayName in ({search})");

            var queryIterator = container
                .GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = GetPartitionKey(organization) });

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
                .GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = GetPartitionKey(organization) });

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

        public async Task<Project> RemoveAsync(Project project)
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

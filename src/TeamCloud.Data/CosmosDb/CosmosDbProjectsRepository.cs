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
using TeamCloud.Model.Internal.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{

    public class CosmosDbProjectsRepository : CosmosDbRepository<Project>, IProjectsRepository
    {
        private readonly IUsersRepository usersRepository;

        public CosmosDbProjectsRepository(ICosmosDbOptions cosmosOptions, IUsersRepository usersRepository)
            : base(cosmosOptions)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
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

                return await PopulateUsersAsync(response.Resource)
                    .ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                throw; // Indicates a name conflict (already a project with name)
            }
        }

        public async Task<Project> GetAsync(string nameOrId)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            Project project;

            try
            {
                var response = await container
                    .ReadItemAsync<Project>(nameOrId, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                project = response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                var query = new QueryDefinition($"SELECT * FROM c WHERE c.name = @name")
                    .WithParameter("@name", nameOrId);

                var queryIterator = container
                    .GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

                var queryResults = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                project = queryResults.FirstOrDefault();
            }

            return await PopulateUsersAsync(project)
                .ConfigureAwait(false);
        }

        public async Task<bool> NameExistsAsync(string name)
        {
            var project = await GetAsync(name)
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
                .UpsertItemAsync(project, new PartitionKey(Options.TenantName))
                .ConfigureAwait(false);

            return await PopulateUsersAsync(response.Resource)
                .ConfigureAwait(false);
        }

        public async IAsyncEnumerable<Project> ListAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM p");

            var queryIterator = container
                .GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var project in queryResponse)
                {
                    yield return await PopulateUsersAsync(project)
                        .ConfigureAwait(false);
                }
            }
        }

        public async IAsyncEnumerable<Project> ListAsync(IEnumerable<string> nameOrIds)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM p WHERE p.id IN (@nameOrIds) OR p.name in (@nameOrIds)")
                .WithParameter("@nameOrIds", nameOrIds);

            var queryIterator = container
                .GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var project in queryResponse)
                {
                    yield return await PopulateUsersAsync(project)
                        .ConfigureAwait(false);
                }
            }
        }


        public async IAsyncEnumerable<Project> ListByProviderAsync(string providerId)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition("SELECT VALUE p FROM p WHERE EXISTS(SELECT VALUE t FROM t IN p.type.providers WHERE t.id = @providerId)")
                .WithParameter("@providerId", providerId);

            var queryIterator = container
                .GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var project in queryResponse)
                {
                    yield return await PopulateUsersAsync(project)
                        .ConfigureAwait(false);
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
                    .DeleteItemAsync<Project>(project.Id, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                await usersRepository
                    .RemoveProjectMembershipsAsync(project.Id)
                    .ConfigureAwait(false);

                return await PopulateUsersAsync(response.Resource)
                    .ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }

        private async Task<Project> PopulateUsersAsync(Project project)
        {
            if (project != null)
            {
                project.Users = await usersRepository
                    .ListAsync(project.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }

            return project;
        }
    }
}

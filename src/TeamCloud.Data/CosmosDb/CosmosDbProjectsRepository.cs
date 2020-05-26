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

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .CreateItemAsync(project)
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                throw; // Indicates a name conflict (already a project with name)
            }
        }

        public async Task<Project> GetAsync(Guid projectId, bool populateUsers = true)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<Project>(projectId.ToString(), new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                var project = response.Resource;

                if (populateUsers)
                    project.Users = await usersRepository
                        .ListAsync(projectId)
                        .ToListAsync()
                        .ConfigureAwait(false);

                return project;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<Project> GetAsync(string nameOrId, bool populateUsers = true)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            if (Guid.TryParse(nameOrId, out var projectId))
                return await GetAsync(projectId, populateUsers)
                    .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.name = @name")
                .WithParameter("@name", nameOrId);

            var queryIterator = container
                .GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            var queryResults = await queryIterator
                .ReadNextAsync()
                .ConfigureAwait(false);

            var project = queryResults.FirstOrDefault();

            if (project != null && populateUsers)
                project.Users = await usersRepository
                    .ListAsync(Guid.Parse(project.Id))
                    .ToListAsync()
                    .ConfigureAwait(false);

            return project;
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

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(project, new PartitionKey(Options.TenantName))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public async IAsyncEnumerable<Project> ListAsync(bool populateUsers = true)
        {
            var users = populateUsers ? await usersRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false) : null;

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
                    if (populateUsers)
                        project.Users = users.Where(u => u.IsAdmin() || u.IsMember(Guid.Parse(project.Id))).ToList();

                    yield return project;
                }
            }
        }

        public async IAsyncEnumerable<Project> ListAsync(IEnumerable<Guid> projectIds, bool populateUsers = true)
        {
            var users = populateUsers ? await usersRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false) : null;

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM p WHERE p.id IN (@projectIds)")
                .WithParameter("@projectIds", projectIds);

            var queryIterator = container
                .GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var project in queryResponse)
                {
                    if (populateUsers)
                        project.Users = users.Where(u => u.IsAdmin() || u.IsMember(Guid.Parse(project.Id))).ToList();

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
                    .DeleteItemAsync<Project>(project.Id, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                await usersRepository
                    .RemoveProjectMembershipsAsync(Guid.Parse(project.Id))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }
    }
}

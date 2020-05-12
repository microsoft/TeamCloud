/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure.Cosmos;
using TeamCloud.Model;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbProjectsRepository : CosmosDbBaseRepository, IProjectsRepository
    {
        private readonly IUsersRepositoryReadOnly usersRepository;

        public CosmosDbProjectsRepository(ICosmosDbOptions cosmosOptions, IUsersRepositoryReadOnly usersRepository)
            : base(cosmosOptions)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        public async Task<Project> AddAsync(Project project)
        {
            var container = await GetContainerAsync<Project>()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .CreateItemAsync(project)
                    .ConfigureAwait(false);

                return response.Value;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                // Indicates a name conflict (already a project with name)
                throw;
            }
        }

        public async Task<Project> GetAsync(Guid projectId, bool populateUsers = true)
        {
            var container = await GetContainerAsync<Project>()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<Project>(projectId.ToString(), new PartitionKey(Constants.CosmosDb.TenantName))
                    .ConfigureAwait(false);

                var project = response.Value;

                if (populateUsers)
                {
                    project.Users = await usersRepository
                        .ListAsync(project.Id)
                        .ToListAsync()
                        .ConfigureAwait(false);
                }

                return project;
            }
            catch (CosmosException cosmosEx)
            {
                if (cosmosEx.Status == (int)HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw;
            }
        }

        public async Task<Project> GetAsync(string name, bool populateUsers = true)
        {
            var container = await GetContainerAsync<Project>()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.name = \"{name}\"");
            var queryIterator = container.GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TenantName) });
            var project = await queryIterator
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (populateUsers)
            {
                project.Users = await usersRepository
                    .ListAsync(project.Id)
                    .ToListAsync()
                    .ConfigureAwait(false);
            }

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
            var container = await GetContainerAsync<Project>()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync<Project>(project, new PartitionKey(Constants.CosmosDb.TenantName))
                .ConfigureAwait(false);

            return response.Value;
        }

        public async IAsyncEnumerable<Project> ListAsync(bool populateUsers = true)
        {
            var users = populateUsers ? await usersRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false) : null;

            var container = await GetContainerAsync<Project>()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");
            var queryIterator = container.GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TenantName) });

            await foreach (var project in queryIterator)
            {
                if (populateUsers)
                    project.Users = users.Where(u => u.IsAdmin() || u.IsOwnerOrMember(project.Id)).ToList();
                yield return project;
            }
        }

        public async Task<Project> RemoveAsync(Project project)
        {
            if (project is null)
                throw new ArgumentNullException(nameof(project));

            var container = await GetContainerAsync<Project>()
                .ConfigureAwait(false);

            var response = await container
                .DeleteItemAsync<Project>(project.Id.ToString(), new PartitionKey(Constants.CosmosDb.TenantName))
                .ConfigureAwait(false);

            return response.Value;
        }
    }
}

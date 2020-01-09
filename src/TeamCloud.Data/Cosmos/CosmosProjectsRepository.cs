/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model;

namespace TeamCloud.Data.Cosmos
{

    public class CosmosProjectsRepository : IProjectsRepository
    {
        private readonly CosmosContainerFactory containerFactory;

        public CosmosProjectsRepository(ICosmosOptions cosmosOptions)
        {
            containerFactory = CosmosContainerFactory.Get(cosmosOptions);
        }

        private Task<Container> GetContainerAsync() 
            => containerFactory.GetContainerAsync<Project>();

        public async Task<Project> AddAsync(Project project)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(project)
                .ConfigureAwait(false);

            return response.Resource;
        }

        public async Task<Project> GetAsync(Guid projectId)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .ReadItemAsync<Project>(projectId.ToString(), new PartitionKey(projectId.ToString()))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public async Task<Project> SetAsync(Project project)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync<Project>(project, new PartitionKey(project.Id.ToString()))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public async IAsyncEnumerable<Project> ListAsync(Guid? userId = null)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");
            var queryIterator = container.GetItemQueryIterator<Project>(query);

            while (queryIterator.HasMoreResults)
            {
                var queryResult = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var project in queryResult.Resource)
                {
                    yield return project;
                }
            }
        }

        public async Task<Project> RemoveAsync(Project project)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .DeleteItemAsync<Project>(project.Id.ToString(), new PartitionKey(project.Id.ToString()))
                .ConfigureAwait(false);

            return response.Resource;
        }
    }
}

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
        public CosmosDbProjectsRepository(ICosmosDbOptions cosmosOptions)
            : base(cosmosOptions)
        { }

        public async Task<Project> AddAsync(Project project)
        {
            var container = await GetContainerAsync<Project>()
                .ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(project)
                .ConfigureAwait(false);

            return response.Value;
        }

        public async Task<Project> GetAsync(Guid projectId)
        {
            var container = await GetContainerAsync<Project>()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<Project>(projectId.ToString(), new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId))
                    .ConfigureAwait(false);

                return response.Value;
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

        public async Task<bool> ExistsAsync(Project project)
        {
            var container = await GetContainerAsync<Project>()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.name = \"{project.Name}\"");
            var queryIterator = container.GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId) });
            var count = await queryIterator.CountAsync();
            return count > 0;
        }

        public async Task<Project> SetAsync(Project project)
        {
            var container = await GetContainerAsync<Project>()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync<Project>(project, new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId))
                .ConfigureAwait(false);

            return response.Value;
        }

        public async IAsyncEnumerable<Project> ListAsync(Guid? userId = null)
        {
            var container = await GetContainerAsync<Project>()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");
            var queryIterator = container.GetItemQueryIterator<Project>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId) });

            await foreach (var queryResult in queryIterator)
            {
                yield return queryResult;
            }
        }

        public async Task<Project> RemoveAsync(Project project)
        {
            var container = await GetContainerAsync<Project>()
                .ConfigureAwait(false);

            var response = await container
                .DeleteItemAsync<Project>(project.Id.ToString(), new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId))
                .ConfigureAwait(false);

            return response.Value;
        }
    }
}

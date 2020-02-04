/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure.Cosmos;
using TeamCloud.Model;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.CosmosDb
{

    public class CosmosDbProjectTypesRepository : CosmosDbBaseRepository, IProjectTypesRepository
    {
        public CosmosDbProjectTypesRepository(ICosmosDbOptions cosmosOptions)
            : base(cosmosOptions)
        { }

        public async Task<ProjectType> AddAsync(ProjectType projectType)
        {
            var container = await GetContainerAsync<ProjectType>()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .CreateItemAsync(projectType)
                    .ConfigureAwait(false);

                return response.Value;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                // Indicates a name conflict (already a ProjectType with name)
                throw;
            }
        }

        public async Task<ProjectType> GetAsync(string id)
        {
            var container = await GetContainerAsync<ProjectType>()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ProjectType>(id, new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId))
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

        public async Task<ProjectType> GetDefaultAsync()
        {
            var container = await GetContainerAsync<ProjectType>()
                .ConfigureAwait(false);

            try
            {
                var query = new QueryDefinition($"SELECT * FROM t WHERE t.default");
                var queryIterator = container.GetItemQueryIterator<ProjectType>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId) });
                var defaultType = await queryIterator
                    .FirstOrDefaultAsync(t => t.Default)
                    .ConfigureAwait(false);

                return defaultType;
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

        public async Task<ProjectType> SetAsync(ProjectType projectType)
        {
            var container = await GetContainerAsync<ProjectType>()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync<ProjectType>(projectType, new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId))
                .ConfigureAwait(false);

            return response.Value;
        }

        public async IAsyncEnumerable<ProjectType> ListAsync()
        {
            var container = await GetContainerAsync<ProjectType>()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");
            var queryIterator = container
                .GetItemQueryIterator<ProjectType>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId) });

            await foreach (var queryResult in queryIterator)
            {
                yield return queryResult;
            }
        }

        public async Task<ProjectType> RemoveAsync(ProjectType projectType)
        {
            var container = await GetContainerAsync<ProjectType>()
                .ConfigureAwait(false);

            var response = await container
                .DeleteItemAsync<ProjectType>(projectType.Id, new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId))
                .ConfigureAwait(false);

            return response.Value;
        }
    }
}

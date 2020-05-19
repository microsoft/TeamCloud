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
using Microsoft.Azure.Cosmos.Linq;
using TeamCloud.Model;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.CosmosDb
{

    public class CosmosDbProjectTypesRepository : CosmosDbBaseRepository<ProjectType>, IProjectTypesRepository
    {
        private readonly IProjectsRepository projectRepository;

        public CosmosDbProjectTypesRepository(ICosmosDbOptions cosmosOptions, IProjectsRepository projectRepository)
            : base(cosmosOptions)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<ProjectType> AddAsync(ProjectType projectType)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .CreateItemAsync(projectType)
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                throw; // Indicates a name conflict (already a ProjectType with name)
            }
        }

        public async Task<ProjectType> GetAsync(string id)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ProjectType>(id, new PartitionKey(Constants.CosmosDb.TenantName))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<int> GetInstanceCountAsync(string id, Guid? subscriptionId = null)
        {
            return await projectRepository.ListAsync()
                .Where(project => project.Type.Id.Equals(id, StringComparison.Ordinal))
                .Where(project => !subscriptionId.HasValue || project.ResourceGroup?.SubscriptionId == subscriptionId.GetValueOrDefault())
                .CountAsync()
                .ConfigureAwait(false);
        }

        public async Task<ProjectType> GetDefaultAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var queryIterator = container.GetItemLinqQueryable<ProjectType>()
                    .Where(projectType => projectType.Default)
                    .ToFeedIterator();

                var queryResults = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                return queryResults.FirstOrDefault();
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<ProjectType> SetAsync(ProjectType projectType)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(projectType, new PartitionKey(Constants.CosmosDb.TenantName))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public async IAsyncEnumerable<ProjectType> ListAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");
            var queryIterator = container.GetItemQueryIterator<ProjectType>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Constants.CosmosDb.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                {
                    yield return queryResult;
                }
            }
        }

        public async Task<ProjectType> RemoveAsync(ProjectType projectType)
        {
            if (projectType is null)
                throw new ArgumentNullException(nameof(projectType));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ProjectType>(projectType.Id, new PartitionKey(Constants.CosmosDb.TenantName))
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

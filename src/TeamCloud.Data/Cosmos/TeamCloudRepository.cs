/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using TeamCloud.Model;

namespace TeamCloud.Data.Cosmos
{
    public class TeamCloudRepository : ITeamCloudRepository
    {
        private readonly ContainerFactory containerFactory;

        private Task<Container> GetContainerAsync() 
            => containerFactory.GetContainerAsync<TeamCloudInstance>();

        public TeamCloudRepository(ICosmosOptions cosmosOptions)
        {
            containerFactory = ContainerFactory.Get(cosmosOptions);
        }

        public async Task<TeamCloudInstance> GetAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .ReadItemAsync<TeamCloudInstance>(Constants.CosmosDb.TeamCloudInstanceId, new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public async Task<TeamCloudInstance> SetAsync(TeamCloudInstance teamCloudInstance)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync<TeamCloudInstance>(teamCloudInstance, new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId))
                .ConfigureAwait(false);

            return response.Resource;
        }

    }
}

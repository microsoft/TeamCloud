/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using TeamCloud.Model;

namespace TeamCloud.Data.Cosmos
{
    public class CosmonsTeamCloudRepository : ITeamCloudRepository
    {
        private readonly CosmosContainerFactory containerFactory;

        private Task<Container> GetContainerAsync() 
            => containerFactory.GetContainerAsync<TeamCloudInstance>();

        public CosmonsTeamCloudRepository(ICosmosOptions cosmosOptions)
        {
            containerFactory = CosmosContainerFactory.Get(cosmosOptions);
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

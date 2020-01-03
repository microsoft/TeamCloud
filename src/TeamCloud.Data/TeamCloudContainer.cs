/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Azure.Cosmos;
using TeamCloud.Model;

namespace TeamCloud.Data
{
    public interface ITeamCloudContainer
    {
        Task<TeamCloudInstance> GetAsync();
    }


    public class TeamCloudContainer : ITeamCloudContainer
    {
        private readonly ContainerFactory containerFactory;

        private Task<Container> GetContainerAsync() => containerFactory.GetContainerAsync<TeamCloudInstance>();

        public TeamCloudContainer(ICosmosOptions cosmosOptions)
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

            return response.Value;
        }
    }
}

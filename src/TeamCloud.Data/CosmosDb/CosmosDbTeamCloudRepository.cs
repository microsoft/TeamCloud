/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Net;
using System.Threading.Tasks;
using Azure.Cosmos;
using TeamCloud.Model;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbTeamCloudRepository : ITeamCloudRepository
    {
        private readonly CosmosDbContainerFactory containerFactory;

        private Task<Container> GetContainerAsync()
            => containerFactory.GetContainerAsync<TeamCloudInstance>();

        public CosmosDbTeamCloudRepository(ICosmosDbOptions cosmosOptions)
        {
            containerFactory = CosmosDbContainerFactory.Get(cosmosOptions);
        }

        public async Task<TeamCloudInstance> GetAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<TeamCloudInstance>(Constants.CosmosDb.TeamCloudInstanceId, new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId))
                    .ConfigureAwait(false);

                return response.Value;
            }
            catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<TeamCloudInstance> SetAsync(TeamCloudInstance teamCloudInstance)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync<TeamCloudInstance>(teamCloudInstance, new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId))
                .ConfigureAwait(false);

            return response.Value;
        }

        public Task<bool> ExistsAsync()
            => GetAsync().ContinueWith(task => !(task.Result is null));
    }
}

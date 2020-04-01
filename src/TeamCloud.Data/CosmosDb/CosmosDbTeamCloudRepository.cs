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
    public class CosmosDbTeamCloudRepository : CosmosDbBaseRepository, ITeamCloudRepository
    {
        public CosmosDbTeamCloudRepository(ICosmosDbOptions cosmosOptions)
            : base(cosmosOptions)
        { }

        public async Task<TeamCloudInstance> GetAsync()
        {
            var container = await GetContainerAsync<TeamCloudInstance>()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<TeamCloudInstance>(Constants.CosmosDb.TenantName, new PartitionKey(Constants.CosmosDb.TenantName))
                    .ConfigureAwait(false);

                return response.Value;
            }
            catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.NotFound)
            {
                return await SetAsync(new TeamCloudInstance())
                    .ConfigureAwait(false);
            }
        }

        public async Task<TeamCloudInstance> SetAsync(TeamCloudInstance teamCloudInstance)
        {
            var container = await GetContainerAsync<TeamCloudInstance>()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync<TeamCloudInstance>(teamCloudInstance, new PartitionKey(Constants.CosmosDb.TenantName))
                .ConfigureAwait(false);

            return response.Value;
        }
    }
}

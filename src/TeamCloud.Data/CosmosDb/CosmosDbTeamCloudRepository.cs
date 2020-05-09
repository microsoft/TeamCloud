/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using TeamCloud.Model;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbTeamCloudRepository : CosmosDbBaseRepository, ITeamCloudRepository
    {
        private readonly IMemoryCache cache;

        public CosmosDbTeamCloudRepository(ICosmosDbOptions cosmosOptions, IMemoryCache cache)
            : base(cosmosOptions)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private TeamCloudInstance SetCache(TeamCloudInstance teamCloud)
        {
            if (teamCloud is null)
            {
                cache.Remove(nameof(TeamCloudInstance));

                return null;
            }

            return cache.Set(nameof(TeamCloudInstance), teamCloud, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5)));
        }

        public Task<TeamCloudInstance> GetAsync()
            => GetAsync(false);

        public async Task<TeamCloudInstance> GetAsync(bool refresh)
        {
            var container = await GetContainerAsync<TeamCloudInstance>()
                .ConfigureAwait(false);

            if (!refresh && this.cache != null && this.cache.TryGetValue<TeamCloudInstance>(nameof(TeamCloudInstance), out var teamCloud))
            { 
                var currentTeamCloud = await FetchAsync((teamCloud as IContainerDocument)?.ETag)
                    .ConfigureAwait(false);

                return currentTeamCloud is null
                    ? teamCloud
                    : currentTeamCloud;
            }
            else
            {
                teamCloud = await FetchAsync()
                    .ConfigureAwait(false);

                return teamCloud is null
                    ? await SetAsync(new TeamCloudInstance()).ConfigureAwait(false)
                    : SetCache(teamCloud);
            }

            async Task<TeamCloudInstance> FetchAsync(string currentETag = default)
            {
                var measure = Stopwatch.StartNew();

                try
                {
                    var options = new ItemRequestOptions()
                    {
                        IfNoneMatchEtag = currentETag
                    };

                    var response = await container
                        .ReadItemAsync<TeamCloudInstance>(Constants.CosmosDb.TenantName, new PartitionKey(Constants.CosmosDb.TenantName), options)
                        .ConfigureAwait(false);

                    return response.ToContainerDocument();
                }
                catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                catch (CosmosException exc) when (exc.StatusCode == HttpStatusCode.NotModified)
                {
                    return null;
                }
                finally
                {
                    Debug.WriteLine($"Fetching '{nameof(TeamCloudInstance)}' took {measure.ElapsedMilliseconds} msec.");
                }
            }
       }

        public async Task<TeamCloudInstance> SetAsync(TeamCloudInstance teamCloudInstance)
        {
            var container = await GetContainerAsync<TeamCloudInstance>()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(teamCloudInstance, new PartitionKey(Constants.CosmosDb.TenantName))
                .ConfigureAwait(false);

            return SetCache(response.ToContainerDocument());
        }
    }
}

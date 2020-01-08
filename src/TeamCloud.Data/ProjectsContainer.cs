/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Cosmos;
using TeamCloud.Model;

namespace TeamCloud.Data
{
    public interface IProjectsContainer
    {
        Task<Project> GetAsync(Guid id);

        IAsyncEnumerable<Project> ListAsync();
    }


    public class ProjectsContainer : IProjectsContainer
    {
        private readonly ContainerFactory containerFactory;

        private Task<Container> GetContainerAsync() => containerFactory.GetContainerAsync<Project>();

        public ProjectsContainer(ICosmosOptions cosmosOptions)
        {
            containerFactory = ContainerFactory.Get(cosmosOptions);
        }

        public async Task<Project> GetAsync(Guid id)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<Project>(id.ToString(), new PartitionKey(Constants.CosmosDb.TeamCloudInstanceId))
                    .ConfigureAwait(false);

                return response.Value;
            }
            catch
            {
                //Console.WriteLine(ex);

                return null;
            }
        }

        public async IAsyncEnumerable<Project> ListAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");

            await foreach (var project in container.GetItemQueryIterator<Project>(query))
            {
                yield return project;
            }
        }
    }
}

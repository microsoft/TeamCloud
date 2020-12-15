using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public sealed class CosmosDbComponentDeploymentRepository : CosmosDbRepository<ComponentDeployment>, IComponentDeploymentRepository
    {
        public CosmosDbComponentDeploymentRepository(ICosmosDbOptions options)
            : base(options)
        { }

        public async Task<ComponentDeployment> AddAsync(ComponentDeployment deployment)
        {
            if (deployment is null)
                throw new ArgumentNullException(nameof(deployment));

            await deployment
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .CreateItemAsync(deployment, GetPartitionKey(deployment))
                .ConfigureAwait(false);

            return response.Resource;
        }

        public async Task<ComponentDeployment> GetAsync(string componentId, string id)
        {
            if (componentId is null)
                throw new ArgumentNullException(nameof(componentId));

            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (!Guid.TryParse(id, out var idParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(id));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ComponentDeployment>(idParsed.ToString(), GetPartitionKey(componentId))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async IAsyncEnumerable<ComponentDeployment> ListAsync(string componentId)
        {
            if (componentId is null)
                throw new ArgumentNullException(nameof(componentId));

            if (!Guid.TryParse(componentId, out var componentIdParsed))
                throw new ArgumentException("Value is not a valid GUID", nameof(componentId));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var queryString = $"SELECT * FROM c WHERE c.componentId = '{componentIdParsed}'";

            var query = new QueryDefinition(queryString);

            var queryIterator = container
                .GetItemQueryIterator<ComponentDeployment>(query, requestOptions: GetQueryRequestOptions(componentId));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async Task RemoveAllAsync(string componentId)
        {
            var components = ListAsync(componentId);

            if (await components.AnyAsync().ConfigureAwait(false))
            {
                var container = await GetContainerAsync()
                    .ConfigureAwait(false);

                var batch = container
                    .CreateTransactionalBatch(GetPartitionKey(componentId));

                await foreach (var component in components.ConfigureAwait(false))
                    batch = batch.DeleteItem(component.Id);

                await batch
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task<ComponentDeployment> RemoveAsync(ComponentDeployment deployment)
        {
            if (deployment is null)
                throw new ArgumentNullException(nameof(deployment));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ComponentDeployment>(deployment.Id, GetPartitionKey(deployment))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }

        public async Task RemoveAsync(string componentId, string id)
        {
            var component = await GetAsync(componentId, id)
                .ConfigureAwait(false);

            if (component != null)
            {
                await RemoveAsync(component)
                    .ConfigureAwait(false);
            }
        }

        public async Task<ComponentDeployment> SetAsync(ComponentDeployment deployment)
        {
            if (deployment is null)
                throw new ArgumentNullException(nameof(deployment));

            await deployment
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var response = await container
                .UpsertItemAsync(deployment, GetPartitionKey(deployment))
                .ConfigureAwait(false);

            return response.Resource;
        }
    }
}

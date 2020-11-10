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
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbDeploymentScopeRepository : CosmosDbRepository<DeploymentScope>, IDeploymentScopeRepository
    {
        public CosmosDbDeploymentScopeRepository(ICosmosDbOptions cosmosOptions)
            : base(cosmosOptions)
        { }

        public async Task<DeploymentScope> AddAsync(DeploymentScope deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            await deploymentScope
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var defaultdeploymentScope = await GetDefaultAsync(deploymentScope.Organization)
                .ConfigureAwait(false);

            if (defaultdeploymentScope is null)
            {
                // ensure we have a default
                // project type if none is defined

                deploymentScope.IsDefault = true;
            }

            try
            {
                if (deploymentScope.IsDefault)
                {
                    var batch = container
                        .CreateTransactionalBatch(GetPartitionKey(deploymentScope))
                        .CreateItem(deploymentScope);

                    var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true and c.id != '{deploymentScope.Id}'");

                    var queryIterator = container
                        .GetItemQueryIterator<DeploymentScope>(query, requestOptions: new QueryRequestOptions { PartitionKey = GetPartitionKey(deploymentScope) });

                    while (queryIterator.HasMoreResults)
                    {
                        var queryResults = await queryIterator
                            .ReadNextAsync()
                            .ConfigureAwait(false);

                        queryResults
                            .Select(qr => { qr.IsDefault = false; return qr; })
                            .ToList()
                            .ForEach(qr => batch.UpsertItem(qr));
                    }

                    var batchResponse = await batch
                        .ExecuteAsync()
                        .ConfigureAwait(false);

                    return await GetAsync(deploymentScope.Organization, deploymentScope.Id)
                        .ConfigureAwait(false);
                }
                else
                {
                    var response = await container
                        .CreateItemAsync(deploymentScope, GetPartitionKey(deploymentScope))
                        .ConfigureAwait(false);

                    return response.Resource;
                }
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                throw; // Indicates a name conflict (already a deploymentScope with name)
            }
        }

        public async Task<DeploymentScope> GetAsync(string organization, string id)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<DeploymentScope>(id, GetPartitionKey(organization))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<DeploymentScope> GetDefaultAsync(string organization)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true");

                var queryIterator = container
                    .GetItemQueryIterator<DeploymentScope>(query, requestOptions: new QueryRequestOptions { PartitionKey = GetPartitionKey(organization) });

                var defaultdeploymentScope = default(DeploymentScope);
                var nonDefaultBatch = default(TransactionalBatch);

                while (queryIterator.HasMoreResults)
                {
                    var queryResults = await queryIterator
                        .ReadNextAsync()
                        .ConfigureAwait(false);

                    defaultdeploymentScope ??= queryResults.Resource.FirstOrDefault();

                    queryResults.Resource
                        .Where(pt => pt.Id != defaultdeploymentScope?.Id)
                        .Select(pt =>
                        {
                            pt.IsDefault = false;
                            return pt;
                        })
                        .ToList()
                        .ForEach(pt =>
                        {
                            nonDefaultBatch ??= container.CreateTransactionalBatch(GetPartitionKey(organization));
                            nonDefaultBatch.UpsertItem(pt);
                        });
                }

                await (nonDefaultBatch?.ExecuteAsync() ?? Task.CompletedTask)
                    .ConfigureAwait(false);

                return defaultdeploymentScope;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<DeploymentScope> SetAsync(DeploymentScope deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            await deploymentScope
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            if (!deploymentScope.IsDefault)
            {
                var defaultdeploymentScope = await GetDefaultAsync(deploymentScope.Organization)
                    .ConfigureAwait(false);

                if (deploymentScope.Id == defaultdeploymentScope?.Id)
                    throw new ArgumentException("One deployment scope must be marked as default");
            }

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            if (deploymentScope.IsDefault)
            {
                var batch = container
                    .CreateTransactionalBatch(GetPartitionKey(deploymentScope))
                    .UpsertItem(deploymentScope);

                var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true and c.id != '{deploymentScope.Id}'");

                var queryIterator = container
                    .GetItemQueryIterator<DeploymentScope>(query, requestOptions: new QueryRequestOptions { PartitionKey = GetPartitionKey(deploymentScope) });

                while (queryIterator.HasMoreResults)
                {
                    var queryResults = await queryIterator
                        .ReadNextAsync()
                        .ConfigureAwait(false);

                    queryResults
                        .Select(qr => { qr.IsDefault = false; return qr; })
                        .ToList()
                        .ForEach(qr => batch.UpsertItem(qr));
                }

                var batchResponse = await batch
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                return await GetAsync(deploymentScope.Organization, deploymentScope.Id)
                    .ConfigureAwait(false);
            }
            else
            {
                var response = await container
                    .UpsertItemAsync(deploymentScope, GetPartitionKey(deploymentScope))
                    .ConfigureAwait(false);

                return response.Resource;
            }
        }

        public async IAsyncEnumerable<DeploymentScope> ListAsync(string organization)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");
            var queryIterator = container
                .GetItemQueryIterator<DeploymentScope>(query, requestOptions: new QueryRequestOptions { PartitionKey = GetPartitionKey(organization) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async Task<DeploymentScope> RemoveAsync(DeploymentScope deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<DeploymentScope>(deploymentScope.Id, GetPartitionKey(deploymentScope))
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

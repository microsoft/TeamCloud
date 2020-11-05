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
    public class CosmosDbDeploymentScopeRepository : CosmosDbRepository<DeploymentScopeDocument>, IDeploymentScopeRepository
    {
        public CosmosDbDeploymentScopeRepository(ICosmosDbOptions cosmosOptions)
            : base(cosmosOptions)
        { }

        public async Task<DeploymentScopeDocument> AddAsync(DeploymentScopeDocument deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            await deploymentScope
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var defaultdeploymentScope = await GetDefaultAsync()
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
                        .CreateTransactionalBatch(new PartitionKey(Options.TenantName))
                        .CreateItem(deploymentScope);

                    var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true and c.id != '{deploymentScope.Id}'");

                    var queryIterator = container
                        .GetItemQueryIterator<DeploymentScopeDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

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

                    return await GetAsync(deploymentScope.Id)
                        .ConfigureAwait(false);
                }
                else
                {
                    var response = await container
                        .CreateItemAsync(deploymentScope, new PartitionKey(Options.TenantName))
                        .ConfigureAwait(false);

                    return response.Resource;
                }
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                throw; // Indicates a name conflict (already a deploymentScope with name)
            }
        }

        public async Task<DeploymentScopeDocument> GetAsync(string id)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<DeploymentScopeDocument>(id, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<DeploymentScopeDocument> GetDefaultAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true");

                var queryIterator = container
                    .GetItemQueryIterator<DeploymentScopeDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

                var defaultdeploymentScope = default(DeploymentScopeDocument);
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
                            nonDefaultBatch ??= container.CreateTransactionalBatch(new PartitionKey(Options.TenantName));
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

        public async Task<DeploymentScopeDocument> SetAsync(DeploymentScopeDocument deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            await deploymentScope
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            if (!deploymentScope.IsDefault)
            {
                var defaultdeploymentScope = await GetDefaultAsync()
                    .ConfigureAwait(false);

                if (deploymentScope.Id == defaultdeploymentScope?.Id)
                    throw new ArgumentException("One deployment scope must be marked as default");
            }

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            if (deploymentScope.IsDefault)
            {
                var batch = container
                    .CreateTransactionalBatch(new PartitionKey(Options.TenantName))
                    .UpsertItem(deploymentScope);

                var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true and c.id != '{deploymentScope.Id}'");

                var queryIterator = container
                    .GetItemQueryIterator<DeploymentScopeDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

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

                return await GetAsync(deploymentScope.Id)
                    .ConfigureAwait(false);
            }
            else
            {
                var response = await container
                    .UpsertItemAsync(deploymentScope, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                return response.Resource;
            }
        }

        public async IAsyncEnumerable<DeploymentScopeDocument> ListAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");
            var queryIterator = container
                .GetItemQueryIterator<DeploymentScopeDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async Task<DeploymentScopeDocument> RemoveAsync(DeploymentScopeDocument deploymentScope)
        {
            if (deploymentScope is null)
                throw new ArgumentNullException(nameof(deploymentScope));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<DeploymentScopeDocument>(deploymentScope.Id, new PartitionKey(Options.TenantName))
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

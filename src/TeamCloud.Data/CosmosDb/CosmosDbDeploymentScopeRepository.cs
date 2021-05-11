/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.Cosmos;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbDeploymentScopeRepository : CosmosDbRepository<DeploymentScope>, IDeploymentScopeRepository
    {
        public CosmosDbDeploymentScopeRepository(ICosmosDbOptions options, IDocumentExpanderProvider expanderProvider = null, IDocumentSubscriptionProvider subscriptionProvider = null, IDataProtectionProvider dataProtectionProvider = null)
            : base(options, expanderProvider, subscriptionProvider, dataProtectionProvider)
        { }

        public override async Task<DeploymentScope> AddAsync(DeploymentScope deploymentScope)
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
                        .GetItemQueryIterator<DeploymentScope>(query, requestOptions: GetQueryRequestOptions(deploymentScope));

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

                    if (batchResponse.IsSuccessStatusCode)
                    {
                        var batchResources = batchResponse.GetOperationResultResources<DeploymentScope>().ToArray();

                        _ = await NotifySubscribersAsync(batchResources.Skip(1), DocumentSubscriptionEvent.Update)
                            .ConfigureAwait(false);

                        return await NotifySubscribersAsync(batchResources.First(), DocumentSubscriptionEvent.Create)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        throw new Exception(batchResponse.ErrorMessage);
                    }
                }
                else
                {
                    var response = await container
                        .CreateItemAsync(deploymentScope, GetPartitionKey(deploymentScope))
                        .ConfigureAwait(false);

                    return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Create)
                        .ConfigureAwait(false);
                }
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                throw; // Indicates a name conflict (already a deploymentScope with name)
            }
        }

        public override async Task<DeploymentScope> GetAsync(string organization, string id, bool expand = false)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<DeploymentScope>(id, GetPartitionKey(organization))
                    .ConfigureAwait(false);

                return await ExpandAsync(response.Resource, expand)
                    .ConfigureAwait(false);
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
                    .GetItemQueryIterator<DeploymentScope>(query, requestOptions: GetQueryRequestOptions(organization));

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

                if (nonDefaultBatch != null)
                {
                    var nonDefaultBatchResponse = await nonDefaultBatch
                        .ExecuteAsync()
                        .ConfigureAwait(false);

                    if (nonDefaultBatchResponse.IsSuccessStatusCode)
                    {
                        _ = await NotifySubscribersAsync(nonDefaultBatchResponse.GetOperationResultResources<DeploymentScope>(), DocumentSubscriptionEvent.Update)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        throw new Exception(nonDefaultBatchResponse.ErrorMessage);
                    }
                }

                return defaultdeploymentScope;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public override async Task<DeploymentScope> SetAsync(DeploymentScope deploymentScope)
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
                    .GetItemQueryIterator<DeploymentScope>(query, requestOptions: GetQueryRequestOptions(deploymentScope));

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

                if (batchResponse.IsSuccessStatusCode)
                {
                    var updatedDeploymentScopes = await NotifySubscribersAsync(batchResponse.GetOperationResultResources<DeploymentScope>(), DocumentSubscriptionEvent.Update)
                        .ConfigureAwait(false);

                    return updatedDeploymentScopes.First();
                }
                else
                {
                    throw new Exception(batchResponse.ErrorMessage);
                }
            }
            else
            {
                var response = await container
                    .UpsertItemAsync(deploymentScope, GetPartitionKey(deploymentScope))
                    .ConfigureAwait(false);

                return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Update)
                    .ConfigureAwait(false);
            }
        }

        public override async IAsyncEnumerable<DeploymentScope> ListAsync(string organization)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");
            var queryIterator = container
                .GetItemQueryIterator<DeploymentScope>(query, requestOptions: GetQueryRequestOptions(organization));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return await ExpandAsync(queryResult).ConfigureAwait(false);
            }
        }

        public override async Task<DeploymentScope> RemoveAsync(DeploymentScope deploymentScope)
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

                return await NotifySubscribersAsync(response.Resource, DocumentSubscriptionEvent.Delete)
                    .ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }
    }
}

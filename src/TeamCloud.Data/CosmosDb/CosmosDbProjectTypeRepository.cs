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
using Microsoft.Azure.Cosmos.Linq;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbProjectTypeRepository : CosmosDbRepository<ProjectTypeDocument>, IProjectTypeRepository
    {
        private readonly IProjectRepository projectRepository;

        public CosmosDbProjectTypeRepository(ICosmosDbOptions cosmosOptions, IProjectRepository projectRepository)
            : base(cosmosOptions)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<ProjectTypeDocument> AddAsync(ProjectTypeDocument projectType)
        {
            if (projectType is null)
                throw new ArgumentNullException(nameof(projectType));

            await projectType
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var defaultProjectType = await GetDefaultAsync()
                .ConfigureAwait(false);

            if (defaultProjectType is null)
            {
                // ensure we have a default
                // project type if none is defined

                projectType.IsDefault = true;
            }

            try
            {
                if (projectType.IsDefault)
                {
                    var batch = container
                        .CreateTransactionalBatch(new PartitionKey(Options.TenantName))
                        .CreateItem(projectType);

                    var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true and c.id != '{projectType.Id}'");

                    var queryIterator = container
                        .GetItemQueryIterator<ProjectTypeDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

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

                    return await GetAsync(projectType.Id)
                        .ConfigureAwait(false);
                }
                else
                {
                    var response = await container
                        .CreateItemAsync(projectType, new PartitionKey(Options.TenantName))
                        .ConfigureAwait(false);

                    return response.Resource;
                }
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                throw; // Indicates a name conflict (already a ProjectType with name)
            }
        }

        public async Task<ProjectTypeDocument> GetAsync(string id)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ProjectTypeDocument>(id, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<int> GetInstanceCountAsync(string id, Guid? subscriptionId = null)
        {
            return await projectRepository.ListAsync()
                .Where(project => project.Type.Id.Equals(id, StringComparison.Ordinal))
                .Where(project => !subscriptionId.HasValue || project.ResourceGroup?.SubscriptionId == subscriptionId.GetValueOrDefault())
                .CountAsync()
                .ConfigureAwait(false);
        }

        public async Task<ProjectTypeDocument> GetDefaultAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true");

                var queryIterator = container
                    .GetItemQueryIterator<ProjectTypeDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

                var defaultProjectType = default(ProjectTypeDocument);
                var nonDefaultBatch = default(TransactionalBatch);

                while (queryIterator.HasMoreResults)
                {
                    var queryResults = await queryIterator
                        .ReadNextAsync()
                        .ConfigureAwait(false);

                    defaultProjectType ??= queryResults.Resource.FirstOrDefault();

                    queryResults.Resource
                        .Where(pt => pt.Id != defaultProjectType?.Id)
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

                return defaultProjectType;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<ProjectTypeDocument> SetAsync(ProjectTypeDocument projectType)
        {
            if (projectType is null)
                throw new ArgumentNullException(nameof(projectType));

            await projectType
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            if (!projectType.IsDefault)
            {
                var defaultProjectType = await GetDefaultAsync()
                    .ConfigureAwait(false);

                if (projectType.Id == defaultProjectType?.Id)
                    throw new ArgumentException("One project type must be marked as default");
            }

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            if (projectType.IsDefault)
            {
                var batch = container
                    .CreateTransactionalBatch(new PartitionKey(Options.TenantName))
                    .UpsertItem(projectType);

                var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true and c.id != '{projectType.Id}'");

                var queryIterator = container
                    .GetItemQueryIterator<ProjectTypeDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

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

                return await GetAsync(projectType.Id)
                    .ConfigureAwait(false);
            }
            else
            {
                var response = await container
                    .UpsertItemAsync(projectType, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                return response.Resource;
            }
        }

        public async IAsyncEnumerable<ProjectTypeDocument> ListAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");
            var queryIterator = container
                .GetItemQueryIterator<ProjectTypeDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async IAsyncEnumerable<ProjectTypeDocument> ListByProviderAsync(string providerId)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT VALUE t FROM t WHERE EXISTS(SELECT VALUE p FROM p IN t.providers WHERE p.id = '{providerId}')");

            var queryIterator = container
                .GetItemQueryIterator<ProjectTypeDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async Task<ProjectTypeDocument> RemoveAsync(ProjectTypeDocument projectType)
        {
            if (projectType is null)
                throw new ArgumentNullException(nameof(projectType));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ProjectTypeDocument>(projectType.Id, new PartitionKey(Options.TenantName))
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

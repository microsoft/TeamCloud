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
    public class CosmosDbProjectTypesRepository : CosmosDbRepository<ProjectType>, IProjectTypesRepository
    {
        private readonly IProjectsRepository projectRepository;

        public CosmosDbProjectTypesRepository(ICosmosDbOptions cosmosOptions, IProjectsRepository projectRepository)
            : base(cosmosOptions)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<ProjectType> AddAsync(ProjectType projectType)
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

                projectType.Default = true;
            }

            try
            {
                if (projectType.Default)
                {
                    var batch = container
                        .CreateTransactionalBatch(new PartitionKey(Options.TenantName))
                        .CreateItem(projectType);

                    var query = new QueryDefinition($"SELECT * FROM c WHERE c.default = true and c.id != @id")
                        .WithParameter("@id", projectType.Id);

                    var queryIterator = container
                        .GetItemQueryIterator<ProjectType>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

                    while (queryIterator.HasMoreResults)
                    {
                        var queryResults = await queryIterator
                            .ReadNextAsync()
                            .ConfigureAwait(false);

                        queryResults
                            .Select(qr => { qr.Default = false; return qr; })
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

        public async Task<ProjectType> GetAsync(string id)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ProjectType>(id, new PartitionKey(Options.TenantName))
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

        public async Task<ProjectType> GetDefaultAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var query = new QueryDefinition($"SELECT * FROM c WHERE c.default = true");

                var queryIterator = container
                    .GetItemQueryIterator<ProjectType>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

                var defaultProjectType = default(ProjectType);
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
                            pt.Default = false;
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

        public async Task<ProjectType> SetAsync(ProjectType projectType)
        {
            if (projectType is null)
                throw new ArgumentNullException(nameof(projectType));

            await projectType
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            if (!projectType.Default)
            {
                var defaultProjectType = await GetDefaultAsync()
                    .ConfigureAwait(false);

                if (projectType.Id == defaultProjectType?.Id)
                    throw new ArgumentException("One project type must be marked as default");
            }

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            if (projectType.Default)
            {
                var batch = container
                    .CreateTransactionalBatch(new PartitionKey(Options.TenantName))
                    .UpsertItem(projectType);

                var query = new QueryDefinition($"SELECT * FROM c WHERE c.default = true and c.id != @id")
                    .WithParameter("@id", projectType.Id);

                var queryIterator = container
                    .GetItemQueryIterator<ProjectType>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

                while (queryIterator.HasMoreResults)
                {
                    var queryResults = await queryIterator
                        .ReadNextAsync()
                        .ConfigureAwait(false);

                    queryResults
                        .Select(qr => { qr.Default = false; return qr; })
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

        public async IAsyncEnumerable<ProjectType> ListAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");
            var queryIterator = container
                .GetItemQueryIterator<ProjectType>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                {
                    yield return queryResult;
                }
            }
        }

        public async IAsyncEnumerable<ProjectType> ListByProviderAsync(string providerId)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition("SELECT VALUE t FROM t WHERE EXISTS(SELECT VALUE p FROM p IN t.providers WHERE p.id = @providerId)")
                .WithParameter("@providerId", providerId);

            var queryIterator = container
                .GetItemQueryIterator<ProjectType>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                {
                    yield return queryResult;
                }
            }
        }

        public async Task<ProjectType> RemoveAsync(ProjectType projectType)
        {
            if (projectType is null)
                throw new ArgumentNullException(nameof(projectType));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ProjectType>(projectType.Id, new PartitionKey(Options.TenantName))
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

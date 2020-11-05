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
    public class CosmosDbProjectTemplateRepository : CosmosDbRepository<ProjectTemplateDocument>, IProjectTemplateRepository
    {
        private readonly IProjectRepository projectRepository;

        public CosmosDbProjectTemplateRepository(ICosmosDbOptions cosmosOptions, IProjectRepository projectRepository)
            : base(cosmosOptions)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        }

        public async Task<ProjectTemplateDocument> AddAsync(ProjectTemplateDocument projectTemplate)
        {
            if (projectTemplate is null)
                throw new ArgumentNullException(nameof(projectTemplate));

            await projectTemplate
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var defaultprojectTemplate = await GetDefaultAsync()
                .ConfigureAwait(false);

            if (defaultprojectTemplate is null)
            {
                // ensure we have a default
                // project type if none is defined

                projectTemplate.IsDefault = true;
            }

            try
            {
                if (projectTemplate.IsDefault)
                {
                    var batch = container
                        .CreateTransactionalBatch(new PartitionKey(Options.TenantName))
                        .CreateItem(projectTemplate);

                    var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true and c.id != '{projectTemplate.Id}'");

                    var queryIterator = container
                        .GetItemQueryIterator<ProjectTemplateDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

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

                    return await GetAsync(projectTemplate.Id)
                        .ConfigureAwait(false);
                }
                else
                {
                    var response = await container
                        .CreateItemAsync(projectTemplate, new PartitionKey(Options.TenantName))
                        .ConfigureAwait(false);

                    return response.Resource;
                }
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                throw; // Indicates a name conflict (already a projectTemplate with name)
            }
        }

        public async Task<ProjectTemplateDocument> GetAsync(string id)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ProjectTemplateDocument>(id, new PartitionKey(Options.TenantName))
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

        public async Task<ProjectTemplateDocument> GetDefaultAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true");

                var queryIterator = container
                    .GetItemQueryIterator<ProjectTemplateDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

                var defaultprojectTemplate = default(ProjectTemplateDocument);
                var nonDefaultBatch = default(TransactionalBatch);

                while (queryIterator.HasMoreResults)
                {
                    var queryResults = await queryIterator
                        .ReadNextAsync()
                        .ConfigureAwait(false);

                    defaultprojectTemplate ??= queryResults.Resource.FirstOrDefault();

                    queryResults.Resource
                        .Where(pt => pt.Id != defaultprojectTemplate?.Id)
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

                return defaultprojectTemplate;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<ProjectTemplateDocument> SetAsync(ProjectTemplateDocument projectTemplate)
        {
            if (projectTemplate is null)
                throw new ArgumentNullException(nameof(projectTemplate));

            await projectTemplate
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            if (!projectTemplate.IsDefault)
            {
                var defaultprojectTemplate = await GetDefaultAsync()
                    .ConfigureAwait(false);

                if (projectTemplate.Id == defaultprojectTemplate?.Id)
                    throw new ArgumentException("One project template must be marked as default");
            }

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            if (projectTemplate.IsDefault)
            {
                var batch = container
                    .CreateTransactionalBatch(new PartitionKey(Options.TenantName))
                    .UpsertItem(projectTemplate);

                var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true and c.id != '{projectTemplate.Id}'");

                var queryIterator = container
                    .GetItemQueryIterator<ProjectTemplateDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

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

                return await GetAsync(projectTemplate.Id)
                    .ConfigureAwait(false);
            }
            else
            {
                var response = await container
                    .UpsertItemAsync(projectTemplate, new PartitionKey(Options.TenantName))
                    .ConfigureAwait(false);

                return response.Resource;
            }
        }

        public async IAsyncEnumerable<ProjectTemplateDocument> ListAsync()
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");
            var queryIterator = container
                .GetItemQueryIterator<ProjectTemplateDocument>(query, requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(Options.TenantName) });

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                foreach (var queryResult in queryResponse)
                    yield return queryResult;
            }
        }

        public async Task<ProjectTemplateDocument> RemoveAsync(ProjectTemplateDocument projectTemplate)
        {
            if (projectTemplate is null)
                throw new ArgumentNullException(nameof(projectTemplate));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ProjectTemplateDocument>(projectTemplate.Id, new PartitionKey(Options.TenantName))
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

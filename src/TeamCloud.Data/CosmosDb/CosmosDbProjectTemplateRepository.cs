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
using Microsoft.Extensions.Caching.Memory;
using TeamCloud.Data.CosmosDb.Core;
using TeamCloud.Git.Services;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbProjectTemplateRepository : CosmosDbRepository<ProjectTemplate>, IProjectTemplateRepository
    {
        private readonly IRepositoryService repositoryService;

        public CosmosDbProjectTemplateRepository(ICosmosDbOptions options, IRepositoryService repositoryService, IMemoryCache cache, IDocumentExpanderProvider expanderProvider = null, IDocumentSubscriptionProvider subscriptionProvider = null, IDataProtectionProvider dataProtectionProvider = null)
            : base(options, expanderProvider, subscriptionProvider, dataProtectionProvider, cache)
        {
            this.repositoryService = repositoryService ?? throw new ArgumentNullException(nameof(repositoryService));
        }

        private Task<ProjectTemplate> AugmentAsync(ProjectTemplate projectTemplate)
            => projectTemplate is null
            ? Task.FromResult(projectTemplate)
            : repositoryService.UpdateProjectTemplateAsync(projectTemplate);

        public override async Task<ProjectTemplate> AddAsync(ProjectTemplate projectTemplate)
        {
            if (projectTemplate is null)
                throw new ArgumentNullException(nameof(projectTemplate));

            await projectTemplate
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var defaultprojectTemplate = await GetDefaultAsync(projectTemplate.Organization)
                .ConfigureAwait(false);

            if (defaultprojectTemplate is null)
            {
                // ensure we have a default
                // project template if none is defined

                projectTemplate.IsDefault = true;
            }

            try
            {
                if (projectTemplate.IsDefault)
                {
                    var batch = container
                        .CreateTransactionalBatch(GetPartitionKey(projectTemplate))
                        .CreateItem(projectTemplate);

                    var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true and c.id != @identifier")
                        .WithParameter("@identifier", projectTemplate.Id);

                    var queryIterator = container
                        .GetItemQueryIterator<ProjectTemplate>(query, requestOptions: GetQueryRequestOptions(projectTemplate));

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
                        var batchResources = batchResponse.GetOperationResultResources<ProjectTemplate>().ToArray();

                        _ = await NotifySubscribersAsync(batchResources.Skip(1), DocumentSubscriptionEvent.Update)
                            .ConfigureAwait(false);

                        projectTemplate = await AugmentAsync(batchResources.First())
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
                        .CreateItemAsync(projectTemplate, GetPartitionKey(projectTemplate))
                        .ConfigureAwait(false);

                    projectTemplate = await AugmentAsync(response.Resource)
                        .ConfigureAwait(false);
                }

                return await NotifySubscribersAsync(projectTemplate, DocumentSubscriptionEvent.Create)
                    .ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                throw; // Indicates a name conflict (already a projectTemplate with name)
            }
        }

        public override Task<ProjectTemplate> GetAsync(string organization, string id, bool expand = false) => GetCachedAsync(organization, id, async cached =>
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            ProjectTemplate projectTemplate = null;

            try
            {
                var response = await container
                    .ReadItemAsync<ProjectTemplate>(id, GetPartitionKey(organization))
                    .ConfigureAwait(false);

                projectTemplate = SetCached(organization, id, response.Resource);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotModified)
            {
                projectTemplate = cached;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            projectTemplate = await AugmentAsync(projectTemplate)
                .ConfigureAwait(false);

            return await ExpandAsync(projectTemplate, expand)
                .ConfigureAwait(false);
        });

        public async Task<ProjectTemplate> GetDefaultAsync(string organization)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true");

                var queryIterator = container
                    .GetItemQueryIterator<ProjectTemplate>(query, requestOptions: GetQueryRequestOptions(organization));

                var defaultprojectTemplate = default(ProjectTemplate);
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
                            nonDefaultBatch ??= container.CreateTransactionalBatch(GetPartitionKey(organization));
                            nonDefaultBatch.UpsertItem(pt);
                        });
                }

                await (nonDefaultBatch?.ExecuteAsync() ?? Task.CompletedTask)
                    .ConfigureAwait(false);

                return await AugmentAsync(defaultprojectTemplate)
                    .ConfigureAwait(false);
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public override async Task<ProjectTemplate> SetAsync(ProjectTemplate projectTemplate)
        {
            if (projectTemplate is null)
                throw new ArgumentNullException(nameof(projectTemplate));

            await projectTemplate
                .ValidateAsync(throwOnValidationError: true)
                .ConfigureAwait(false);

            if (!projectTemplate.IsDefault)
            {
                var defaultprojectTemplate = await GetDefaultAsync(projectTemplate.Organization)
                    .ConfigureAwait(false);

                if (projectTemplate.Id == defaultprojectTemplate?.Id)
                    throw new ArgumentException("One project template must be marked as default");
            }

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            if (projectTemplate.IsDefault)
            {
                var batch = container
                    .CreateTransactionalBatch(GetPartitionKey(projectTemplate))
                    .UpsertItem(projectTemplate);

                var query = new QueryDefinition($"SELECT * FROM c WHERE c.isDefault = true and c.id != '{projectTemplate.Id}'");

                var queryIterator = container
                    .GetItemQueryIterator<ProjectTemplate>(query, requestOptions: GetQueryRequestOptions(projectTemplate));

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
                    var batchResources = batchResponse.GetOperationResultResources<ProjectTemplate>().ToArray();

                    _ = await NotifySubscribersAsync(batchResources.Skip(1), DocumentSubscriptionEvent.Update)
                        .ConfigureAwait(false);

                    projectTemplate = await AugmentAsync(batchResources.First())
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
                    .UpsertItemAsync(projectTemplate, GetPartitionKey(projectTemplate))
                    .ConfigureAwait(false);

                projectTemplate = await AugmentAsync(response.Resource)
                    .ConfigureAwait(false);
            }

            return await NotifySubscribersAsync(projectTemplate, DocumentSubscriptionEvent.Update)
                .ConfigureAwait(false);
        }

        public override async IAsyncEnumerable<ProjectTemplate> ListAsync(string organization)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");

            var projects = container
                .GetItemQueryIterator<ProjectTemplate>(query, requestOptions: GetQueryRequestOptions(organization))
                .ReadAllAsync(item => repositoryService.UpdateProjectTemplateAsync(item).ContinueWith(t => ExpandAsync(t.Result), TaskContinuationOptions.OnlyOnRanToCompletion).Unwrap())
                .ConfigureAwait(false);

            await foreach (var project in projects)
                yield return project;
        }

        public override async Task<ProjectTemplate> RemoveAsync(ProjectTemplate projectTemplate)
        {
            if (projectTemplate is null)
                throw new ArgumentNullException(nameof(projectTemplate));

            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .DeleteItemAsync<ProjectTemplate>(projectTemplate.Id, GetPartitionKey(projectTemplate))
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

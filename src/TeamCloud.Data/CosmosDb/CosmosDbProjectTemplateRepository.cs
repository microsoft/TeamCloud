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
using TeamCloud.Git.Services;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbProjectTemplateRepository : CosmosDbRepository<ProjectTemplate>, IProjectTemplateRepository
    {
        private readonly IRepositoryService repositoryService;

        public CosmosDbProjectTemplateRepository(ICosmosDbOptions options, IDocumentExpanderProvider expanderProvider, IRepositoryService repositoryService, IDataProtectionProvider dataProtectionProvider = null)
            : base(options, expanderProvider, dataProtectionProvider)
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

                    return await GetAsync(projectTemplate.Organization, projectTemplate.Id)
                        .ConfigureAwait(false);
                }
                else
                {
                    var response = await container
                        .CreateItemAsync(projectTemplate, GetPartitionKey(projectTemplate))
                        .ConfigureAwait(false);

                    return await AugmentAsync(response.Resource)
                        .ConfigureAwait(false);
                }
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.Conflict)
            {
                throw; // Indicates a name conflict (already a projectTemplate with name)
            }
        }

        public override async Task<ProjectTemplate> GetAsync(string organization, string id, bool expand = false)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            try
            {
                var response = await container
                    .ReadItemAsync<ProjectTemplate>(id, GetPartitionKey(organization))
                    .ConfigureAwait(false);

                var document = await AugmentAsync(response.Resource)
                    .ConfigureAwait(false);

                if (expand)
                {
                    document = await ExpandAsync(document)
                        .ConfigureAwait(false);
                }

                return document;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

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

                return await GetAsync(projectTemplate.Organization, projectTemplate.Id)
                    .ConfigureAwait(false);
            }
            else
            {
                var response = await container
                    .UpsertItemAsync(projectTemplate, GetPartitionKey(projectTemplate))
                    .ConfigureAwait(false);

                return await AugmentAsync(response.Resource)
                    .ConfigureAwait(false);
            }
        }

        public override async IAsyncEnumerable<ProjectTemplate> ListAsync(string organization)
        {
            var container = await GetContainerAsync()
                .ConfigureAwait(false);

            var query = new QueryDefinition($"SELECT * FROM c");
            var queryIterator = container
                .GetItemQueryIterator<ProjectTemplate>(query, requestOptions: GetQueryRequestOptions(organization));

            while (queryIterator.HasMoreResults)
            {
                var queryResponse = await queryIterator
                    .ReadNextAsync()
                    .ConfigureAwait(false);

                var tasks = queryResponse
                    .Select(qr => repositoryService.UpdateProjectTemplateAsync(qr))
                    .ToList();

                while (tasks.Any())
                {
                    var completed = await Task
                        .WhenAny(tasks)
                        .ConfigureAwait(false);

                    yield return completed.Result;

                    tasks.Remove(completed);
                }
            }
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

                return response.Resource;
            }
            catch (CosmosException cosmosEx) when (cosmosEx.StatusCode == HttpStatusCode.NotFound)
            {
                return null; // already deleted
            }
        }
    }
}

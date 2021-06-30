/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TeamCloud.Git.Services;
using TeamCloud.Model.Data;

namespace TeamCloud.Data.CosmosDb
{
    public class CosmosDbComponentTemplateRepository : IComponentTemplateRepository
    {
        private readonly IRepositoryService repositoryService;
        private readonly IProjectRepository projectRepository;
        private readonly IProjectTemplateRepository projectTemplateRepository;
        private readonly IMemoryCache cache;

        public CosmosDbComponentTemplateRepository(IRepositoryService repositoryService, IProjectRepository projectRepository, IProjectTemplateRepository projectTemplateRepository, IMemoryCache cache)
        {
            this.repositoryService = repositoryService ?? throw new ArgumentNullException(nameof(repositoryService));
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.projectTemplateRepository = projectTemplateRepository ?? throw new ArgumentNullException(nameof(projectTemplateRepository));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private Task<string> ResolveProjectTemplateIdAsync(string organization, string projectId)
        {
            var cacheKey = $"{this.GetType().Name}|{organization}|{projectId}";

            return cache.GetOrCreateAsync(cacheKey, async (entry) =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(5));

                var project = await projectRepository
                    .GetAsync(organization, projectId)
                    .ConfigureAwait(false);

                return project.Template;
            });
        }

        public async Task<ComponentTemplate> GetAsync(string organization, string projectId, string id)
        {
            var templateId = await ResolveProjectTemplateIdAsync(organization, projectId)
                .ConfigureAwait(false);

            var projectTemplate = await projectTemplateRepository
                .GetAsync(organization, templateId)
                .ConfigureAwait(false);

            return await repositoryService
                .GetComponentTemplateAsync(projectTemplate, id)
                .ConfigureAwait(false);
        }

        public async IAsyncEnumerable<ComponentTemplate> ListAsync(string organization, string projectId)
        {
            var templateId = await ResolveProjectTemplateIdAsync(organization, projectId)
                .ConfigureAwait(false);

            var projectTemplate = await projectTemplateRepository
                .GetAsync(organization, templateId)
                .ConfigureAwait(false);

            await foreach (var componentTemplate in repositoryService.GetComponentTemplatesAsync(projectTemplate))
                yield return componentTemplate;
        }


        public async IAsyncEnumerable<ComponentTemplate> ListAsync(string organization, string projectId, IEnumerable<string> identifiers)
        {
            var templateId = await ResolveProjectTemplateIdAsync(organization, projectId)
                .ConfigureAwait(false);

            var projectTemplate = await projectTemplateRepository
                .GetAsync(organization, templateId)
                .ConfigureAwait(false);

            await foreach (var componentTemplate in repositoryService.GetComponentTemplatesAsync(projectTemplate))
                if (identifiers.Any(i => i == componentTemplate.Id))
                    yield return componentTemplate;
        }
    }
}

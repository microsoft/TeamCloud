/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Git.Caching;
using TeamCloud.Model.Data;

namespace TeamCloud.Git.Services
{
    public class RepositoryService : IRepositoryService
    {
        private readonly GitHubService github;
        private readonly DevOpsService devops;

        public RepositoryService(IRepositoryCache cache)
        {
            github = new GitHubService(cache);
            devops = new DevOpsService();
        }

        public Task<RepositoryReference> GetRepositoryReferenceAsync(RepositoryReference repository)
        {
            repository = repository.ParseUrl();

            return repository.Provider switch
            {
                RepositoryProvider.DevOps => DevOpsService.GetRepositoryReferenceAsync(repository),
                RepositoryProvider.GitHub => github.GetRepositoryReferenceAsync(repository),
                _ => throw new NotSupportedException($"Repository provider {repository.Provider} is not supported.")
            };
        }

        public async Task<ProjectTemplate> UpdateProjectTemplateAsync(ProjectTemplate projectTemplate)
        {
            if (projectTemplate is null)
                throw new ArgumentNullException(nameof(projectTemplate));

            if (projectTemplate.Repository.Provider == RepositoryProvider.Unknown || projectTemplate.Repository.Type == RepositoryReferenceType.Branch)
                projectTemplate.Repository = await GetRepositoryReferenceAsync(projectTemplate.Repository).ConfigureAwait(false);

            return await (projectTemplate.Repository.Provider switch
            {
                RepositoryProvider.DevOps => devops.UpdateProjectTemplateAsync(projectTemplate),
                RepositoryProvider.GitHub => github.UpdateProjectTemplateAsync(projectTemplate),
                _ => throw new NotSupportedException($"Repository provider {projectTemplate.Repository.Provider} is not supported.")

            }).ConfigureAwait(false);
        }

        public Task<ComponentTemplate> GetComponentTemplateAsync(ProjectTemplate projectTemplate, string templateId)
        {
            if (projectTemplate is null)
                throw new ArgumentNullException(nameof(projectTemplate));

            return projectTemplate.Repository.Provider switch
            {
                RepositoryProvider.DevOps => devops.GetComponentTemplateAsync(projectTemplate, templateId),
                RepositoryProvider.GitHub => github.GetComponentTemplateAsync(projectTemplate, templateId),
                _ => throw new NotSupportedException($"Repository provider {projectTemplate.Repository.Provider} is not supported.")
            };
        }

        public IAsyncEnumerable<ComponentTemplate> GetComponentTemplatesAsync(ProjectTemplate projectTemplate)
        {
            if (projectTemplate is null)
                throw new ArgumentNullException(nameof(projectTemplate));

            return projectTemplate.Repository.Provider switch
            {
                RepositoryProvider.DevOps => devops.GetComponentTemplatesAsync(projectTemplate),
                RepositoryProvider.GitHub => github.GetComponentTemplatesAsync(projectTemplate),
                _ => throw new NotSupportedException($"Repository provider {projectTemplate.Repository.Provider} is not supported.")
            };
        }

    }
}

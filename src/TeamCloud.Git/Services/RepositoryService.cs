/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
// using TeamCloud.Git.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Git.Services
{
    public class RepositoryService : IRepositoryService
    {
        private readonly GitHubService github;
        private readonly DevOpsService devops;

        public RepositoryService()
        {
            github = new GitHubService();
            devops = new DevOpsService();
        }

        public Task<RepositoryReference> GetRepositoryReferenceAsync(string url, string version, string token)
        {
            var repository = new RepositoryReference
            {
                Url = url,
                Token = token,
                Version = version
            }
            .ParseUrl();

            return GetRepositoryReferenceAsync(repository);
        }

        private Task<RepositoryReference> GetRepositoryReferenceAsync(RepositoryReference repository) => repository?.Provider switch
        {
            RepositoryProvider.DevOps => devops.GetRepositoryReferenceAsync(repository),
            RepositoryProvider.GitHub => github.GetRepositoryReferenceAsync(repository),
            _ => throw new NotSupportedException("Only GitHub and Azure DevOps git repositories are supported. Generic git repositories are not supported.")
        } ?? throw new ArgumentNullException(nameof(repository));

        public Task<ProjectTemplate> GetProjectTemplateAsync(RepositoryReference repository) => repository?.Provider switch
        {
            RepositoryProvider.DevOps => devops.GetProjectTemplateAsync(repository),
            RepositoryProvider.GitHub => github.GetProjectTemplateAsync(repository),
            _ => throw new NotSupportedException("Only GitHub and Azure DevOps git repositories are supported. Generic git repositories are not supported.")
        } ?? throw new ArgumentNullException(nameof(repository));

        public Task<List<ComponentOffer>> GetComponentOffersAsync(RepositoryReference repository) => repository?.Provider switch
        {
            RepositoryProvider.DevOps => devops.GetComponentOffersAsync(repository),
            RepositoryProvider.GitHub => github.GetComponentOffersAsync(repository),
            _ => throw new NotSupportedException("Only GitHub and Azure DevOps git repositories are supported. Generic git repositories are not supported.")
        } ?? throw new ArgumentNullException(nameof(repository));

        // public Task<ProjectTemplateRepository> GetProjectTemplateRepositoryAsync(RepositoryReference repository) => repository?.Provider switch
        // {
        //     RepositoryProvider.DevOps => devops.GetProjectTemplateRepositoryAsync(repository),
        //     RepositoryProvider.GitHub => github.GetProjectTemplateRepositoryAsync(repository),
        //     _ => throw new NotSupportedException("Only GitHub and Azure DevOps git repositories are supported. Generic git repositories are not supported.")
        // } ?? throw new ArgumentNullException(nameof(repository));
    }
}

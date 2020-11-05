/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using TeamCloud.Git.Data;
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

        public Task<ProjectTemplateDefinition> GetProjectTemplateDefinitionAsync(RepositoryReference repository)
        {
            if (repository is null)
                throw new ArgumentNullException(nameof(repository));

            if (repository.IsGitHub())
                return github.GetProjectTemplateDefinitionAsync(repository);

            if (repository.IsDevOps())
                return devops.GetProjectTemplateDefinitionAsync(repository);

            throw new NotSupportedException("Generic git repositories are not supported.");
        }
    }
}

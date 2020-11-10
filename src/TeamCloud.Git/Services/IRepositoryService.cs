/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Git.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Git.Services
{
    public interface IRepositoryService
    {
        Task<RepositoryReference> GetRepositoryReferenceAsync(string url, string version, string token);

        Task<ProjectTemplate> GetProjectTemplateAsync(RepositoryReference repository);

        Task<List<ComponentOffer>> GetComponentOffersAsync(RepositoryReference repository);

        // Task<ProjectTemplateRepository> GetProjectTemplateRepositoryAsync(RepositoryReference repository);
    }
}

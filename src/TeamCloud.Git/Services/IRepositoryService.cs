/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Git.Services
{
    public interface IRepositoryService
    {
        Task<RepositoryReference> GetRepositoryReferenceAsync(string url, string version, string token);

        Task<ProjectTemplate> UpdateProjectTemplateAsync(ProjectTemplate projectTemplate);

        Task<List<ComponentTemplate>> GetComponentTemplatesAsync(ProjectTemplate projectTemplate);
    }
}

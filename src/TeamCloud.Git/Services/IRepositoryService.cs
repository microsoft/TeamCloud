/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Git.Data;
using TeamCloud.Model.Data;

namespace TeamCloud.Git.Services
{
    public interface IRepositoryService
    {
        Task<ProjectTemplateDefinition> GetProjectTemplateDefinitionAsync(RepositoryReference repository);
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IProjectTemplateRepository
    {
        Task<ProjectTemplate> GetAsync(string organization, string id);

        IAsyncEnumerable<ProjectTemplate> ListAsync(string organization);

        // Task<int> GetInstanceCountAsync(string id, Guid? subscriptionId = null);

        Task<ProjectTemplate> GetDefaultAsync(string organization);

        Task<ProjectTemplate> AddAsync(ProjectTemplate projectTemplate);

        Task<ProjectTemplate> SetAsync(ProjectTemplate projectTemplate);

        Task<ProjectTemplate> RemoveAsync(ProjectTemplate projectTemplate);
    }
}

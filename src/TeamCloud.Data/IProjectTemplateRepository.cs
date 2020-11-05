/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IProjectTemplateRepository
    {
        Task<ProjectTemplateDocument> GetAsync(string id);

        IAsyncEnumerable<ProjectTemplateDocument> ListAsync();

        Task<int> GetInstanceCountAsync(string id, Guid? subscriptionId = null);

        Task<ProjectTemplateDocument> GetDefaultAsync();

        Task<ProjectTemplateDocument> AddAsync(ProjectTemplateDocument projectTemplate);

        Task<ProjectTemplateDocument> SetAsync(ProjectTemplateDocument projectTemplate);

        Task<ProjectTemplateDocument> RemoveAsync(ProjectTemplateDocument projectTemplate);
    }
}

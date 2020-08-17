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
    public interface IProjectTypesRepository
    {
        Task<ProjectTypeDocument> GetAsync(string id);

        IAsyncEnumerable<ProjectTypeDocument> ListAsync();

        IAsyncEnumerable<ProjectTypeDocument> ListByProviderAsync(string providerId);

        Task<int> GetInstanceCountAsync(string id, Guid? subscriptionId = null);

        Task<ProjectTypeDocument> GetDefaultAsync();

        Task<ProjectTypeDocument> AddAsync(ProjectTypeDocument projectType);

        Task<ProjectTypeDocument> SetAsync(ProjectTypeDocument projectType);

        Task<ProjectTypeDocument> RemoveAsync(ProjectTypeDocument projectType);
    }
}

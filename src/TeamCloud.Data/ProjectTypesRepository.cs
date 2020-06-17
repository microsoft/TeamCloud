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
        Task<ProjectType> GetAsync(string id);

        IAsyncEnumerable<ProjectType> ListAsync();

        IAsyncEnumerable<ProjectType> ListByProviderAsync(string providerId);

        Task<int> GetInstanceCountAsync(string id, Guid? subscriptionId = null);

        Task<ProjectType> GetDefaultAsync();

        Task<ProjectType> AddAsync(ProjectType projectType);

        Task<ProjectType> SetAsync(ProjectType projectType);

        Task<ProjectType> RemoveAsync(ProjectType projectType);
    }
}

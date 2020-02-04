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
    public interface IProjectTypesRepositoryReadOnly
    {
        Task<ProjectType> GetAsync(string id);

        IAsyncEnumerable<ProjectType> ListAsync();

        Task<ProjectType> GetDefaultAsync();
    }

    public interface IProjectTypesRepository : IProjectTypesRepositoryReadOnly
    {
        Task<ProjectType> AddAsync(ProjectType projectType);

        Task<ProjectType> SetAsync(ProjectType projectType);

        Task<ProjectType> RemoveAsync(ProjectType projectType);
    }
}

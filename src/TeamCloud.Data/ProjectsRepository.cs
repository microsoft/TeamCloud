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
    public interface IProjectsRepositoryReadOnly
    {
        Task<Project> GetAsync(Guid id, bool populateUsers = true);

        Task<Project> GetAsync(string name, bool populateUsers = true);

        IAsyncEnumerable<Project> ListAsync(bool populateUsers = true);

        Task<bool> NameExistsAsync(string name);
    }

    public interface IProjectsRepository : IProjectsRepositoryReadOnly
    {
        Task<Project> AddAsync(Project project);

        Task<Project> SetAsync(Project project);

        Task<Project> RemoveAsync(Project project);
    }
}

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
    public interface IProjectsRepository
    {
        Task<Project> GetAsync(string nameOrId, bool populateUsers = true);

        IAsyncEnumerable<Project> ListAsync(bool populateUsers = true);

        IAsyncEnumerable<Project> ListAsync(IEnumerable<string> projectIds, bool populateUsers = true);

        Task<bool> NameExistsAsync(string name);

        Task<Project> AddAsync(Project project);

        Task<Project> SetAsync(Project project);

        Task<Project> RemoveAsync(Project project);
    }
}

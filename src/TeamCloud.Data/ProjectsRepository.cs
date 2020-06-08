/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IProjectsRepository
    {
        Task<Project> GetAsync(string nameOrId);

        IAsyncEnumerable<Project> ListAsync();

        IAsyncEnumerable<Project> ListAsync(IEnumerable<string> nameOrIds);

        Task<bool> NameExistsAsync(string name);

        Task<Project> AddAsync(Project project);

        Task<Project> SetAsync(Project project);

        Task<Project> RemoveAsync(Project project);
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Data
{
    public interface IProjectsRepository
    {
        Task<ProjectDocument> GetAsync(string nameOrId);

        IAsyncEnumerable<ProjectDocument> ListAsync();

        IAsyncEnumerable<ProjectDocument> ListAsync(IEnumerable<string> nameOrIds);

        IAsyncEnumerable<ProjectDocument> ListByProviderAsync(string providerId);

        Task<bool> NameExistsAsync(string name);

        Task<ProjectDocument> AddAsync(ProjectDocument project);

        Task<ProjectDocument> SetAsync(ProjectDocument project);

        Task<ProjectDocument> RemoveAsync(ProjectDocument project);
    }
}

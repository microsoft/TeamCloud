/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IProjectLinkRepository
    {
        Task<ProjectLinkDocument> GetAsync(string projectId, string id);

        IAsyncEnumerable<ProjectLinkDocument> ListAsync(string projectId);

        Task<ProjectLinkDocument> AddAsync(ProjectLinkDocument projectLink);

        Task<ProjectLinkDocument> SetAsync(ProjectLinkDocument projectLink);

        Task<ProjectLinkDocument> RemoveAsync(ProjectLinkDocument projectLink);

        Task RemoveAsync(string projectId);

        Task RemoveAsync(string projectId, string id);
    }
}

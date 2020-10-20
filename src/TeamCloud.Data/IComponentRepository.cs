/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IComponentRepository
    {
        Task<ComponentDocument> GetAsync(string projectId, string id);

        IAsyncEnumerable<ComponentDocument> ListAsync(string projectId, string providerId = null);

        Task<ComponentDocument> AddAsync(ComponentDocument component);

        Task<ComponentDocument> SetAsync(ComponentDocument component);

        Task<ComponentDocument> RemoveAsync(ComponentDocument component);

        Task RemoveAsync(string projectId);

        Task RemoveAsync(string projectId, string id);
    }
}

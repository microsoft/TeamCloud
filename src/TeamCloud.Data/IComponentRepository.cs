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
        Task<Component> AddAsync(Component component);

        Task<Component> GetAsync(string projectId, string id);

        IAsyncEnumerable<Component> ListAsync(string projectId);

        Task<Component> SetAsync(Component component);

        Task<Component> RemoveAsync(Component component);

        Task RemoveAllAsync(string projectId);

        Task RemoveAsync(string projectId, string id);
    }
}

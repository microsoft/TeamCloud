/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IComponentTemplateRepository
    {
        Task<ComponentTemplate> GetAsync(string organization, string id);

        IAsyncEnumerable<ComponentTemplate> ListAsync(string organization);

        IAsyncEnumerable<ComponentTemplate> ListAsync(string organization, string parentId);

        IAsyncEnumerable<ComponentTemplate> ListAsync(string organization, IEnumerable<string> parentIds);

        Task<ComponentTemplate> AddAsync(ComponentTemplate componentTemplate);

        Task<ComponentTemplate> SetAsync(ComponentTemplate componentTemplate);

        Task<ComponentTemplate> RemoveAsync(ComponentTemplate componentTemplate);
    }
}

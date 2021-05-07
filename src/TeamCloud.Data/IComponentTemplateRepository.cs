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
        Task<ComponentTemplate> GetAsync(string organization, string projectId, string id);

        IAsyncEnumerable<ComponentTemplate> ListAsync(string organization, string projectId);

        IAsyncEnumerable<ComponentTemplate> ListAsync(string organization, string projectId, IEnumerable<string> identifiers);
    }
}

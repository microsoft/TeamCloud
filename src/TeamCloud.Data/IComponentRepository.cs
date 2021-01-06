/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IComponentRepository : IDocumentRepository<Component>
    {
        Task RemoveAllAsync(string projectId);
    }
}

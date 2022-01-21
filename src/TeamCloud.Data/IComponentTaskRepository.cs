/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data;

public interface IComponentTaskRepository : IDocumentRepository<ComponentTask>
{
    Task RemoveAllAsync(string componentId);

    Task RemoveAsync(string componentId, string id);
}

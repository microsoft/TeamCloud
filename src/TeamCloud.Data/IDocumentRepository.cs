/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Data
{
    public interface IDocumentRepository<T>
        where T : class, IContainerDocument, new()
    {
        Task<T> AddAsync(T document);

        Task<T> GetAsync(string partitionId, string documentId, bool expand = false);

        Task<T> SetAsync(T document);

        IAsyncEnumerable<T> ListAsync(string partitionId);

        Task<T> RemoveAsync(T document);

        Task<T> ExpandAsync(T document, bool includeOptional = false);
    }
}

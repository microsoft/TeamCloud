/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IProviderDataRepository
    {

        Task<ProviderDataDocument> AddAsync(ProviderDataDocument data);

        Task<ProviderDataDocument> GetAsync(string id);

        Task<ProviderDataDocument> GetAsync(string providerId, string nameOrId);

        IAsyncEnumerable<ProviderDataDocument> GetByNameAsync(string providerId, string Name);

        IAsyncEnumerable<ProviderDataDocument> GetByNameAsync(string providerId, string projectId, string name);

        Task<ProviderDataDocument> SetAsync(ProviderDataDocument data);

        IAsyncEnumerable<ProviderDataDocument> ListAsync(string providerId, bool includeShared = false);

        IAsyncEnumerable<ProviderDataDocument> ListAsync(string providerId, string projectId, bool includeShared = false);

        Task<ProviderDataDocument> RemoveAsync(ProviderDataDocument data);

        Task RemoveAsync(string projectId);
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Data
{
    public interface IProviderDataRepository
    {

        Task<ProviderData> AddAsync(ProviderData data);

        Task<ProviderData> GetAsync(string id);

        Task<ProviderData> GetAsync(string providerId, string nameOrId);

        IAsyncEnumerable<ProviderData> GetByNameAsync(string providerId, string Name);

        IAsyncEnumerable<ProviderData> GetByNameAsync(string providerId, string projectId, string name);

        Task<ProviderData> SetAsync(ProviderData data);

        IAsyncEnumerable<ProviderData> ListAsync(string providerId, bool includeShared = false);

        IAsyncEnumerable<ProviderData> ListAsync(string providerId, string projectId, bool includeShared = false);

        Task<ProviderData> RemoveAsync(ProviderData data);

        Task RemoveAsync(string projectId);
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IProvidersRepository
    {
        Task<ProviderDocument> GetAsync(string id);

        IAsyncEnumerable<ProviderDocument> ListAsync();

        IAsyncEnumerable<ProviderDocument> ListAsync(IEnumerable<string> ids);

        Task<ProviderDocument> AddAsync(ProviderDocument provider);

        Task<ProviderDocument> SetAsync(ProviderDocument provider);

        Task<ProviderDocument> RemoveAsync(ProviderDocument provider);
    }
}

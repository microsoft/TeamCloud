/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IProvidersRepository
    {
        Task<Provider> GetAsync(string id);

        IAsyncEnumerable<Provider> ListAsync();

        Task<Provider> AddAsync(Provider provider);

        Task<Provider> SetAsync(Provider provider);

        Task<Provider> RemoveAsync(Provider provider);
    }
}

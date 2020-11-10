/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IComponentOfferRepository
    {
        Task<ComponentOffer> GetAsync(string id);

        IAsyncEnumerable<ComponentOffer> ListAsync();

        IAsyncEnumerable<ComponentOffer> ListAsync(string providerId);

        IAsyncEnumerable<ComponentOffer> ListAsync(IEnumerable<string> providerIds);

        Task<ComponentOffer> AddAsync(ComponentOffer componentOffer);

        Task<ComponentOffer> SetAsync(ComponentOffer componentOffer);

        Task<ComponentOffer> RemoveAsync(ComponentOffer componentOffer);
    }
}

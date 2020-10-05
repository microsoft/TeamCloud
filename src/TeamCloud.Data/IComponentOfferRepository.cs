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
        Task<ComponentOfferDocument> GetAsync(string id);

        IAsyncEnumerable<ComponentOfferDocument> ListAsync();

        IAsyncEnumerable<ComponentOfferDocument> ListAsync(string providerId);

        IAsyncEnumerable<ComponentOfferDocument> ListAsync(IEnumerable<string> providerIds);

        Task<ComponentOfferDocument> AddAsync(ComponentOfferDocument componentOffer);

        Task<ComponentOfferDocument> SetAsync(ComponentOfferDocument componentOffer);

        Task<ComponentOfferDocument> RemoveAsync(ComponentOfferDocument componentOffer);
    }
}

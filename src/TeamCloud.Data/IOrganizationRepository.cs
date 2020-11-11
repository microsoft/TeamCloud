/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IOrganizationRepository
    {
        Task<Organization> GetAsync(string tenant, string id);

        Task<string> ResolveIdAsync(string tenant, string identifier);

        IAsyncEnumerable<Organization> ListAsync(string tenant);

        Task<Organization> AddAsync(Organization organization);

        Task<Organization> SetAsync(Organization organization);

        Task<Organization> RemoveAsync(Organization organization);
    }
}

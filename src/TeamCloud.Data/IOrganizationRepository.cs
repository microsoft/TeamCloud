/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IOrganizationRepository : IDocumentRepository<Organization>
    {
        Task<string> ResolveIdAsync(string tenant, string identifier);

        IAsyncEnumerable<Organization> ListAsync(string tenant, IEnumerable<string> identifiers);
    }
}

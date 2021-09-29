/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IProjectRepository : IDocumentRepository<Project>
    {
        Task<string> ResolveIdAsync(string tenant, string identifier);

        Task<Project> RemoveAsync(Project project, bool soft);

        IAsyncEnumerable<Project> ListAsync(string organization, IEnumerable<string> identifiers);

        IAsyncEnumerable<Project> ListByTemplateAsync(string organization, string templateId);

        Task<bool> NameExistsAsync(string organization, string name);
    }
}

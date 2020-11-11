/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IProjectRepository
    {
        Task<Project> GetAsync(string organization, string identifier);

        Task<string> ResolveIdAsync(string tenant, string identifier);

        IAsyncEnumerable<Project> ListAsync(string organization);

        IAsyncEnumerable<Project> ListAsync(string organization, IEnumerable<string> identifiers);

        IAsyncEnumerable<Project> ListByTemplateAsync(string organization, string templateId);

        Task<bool> NameExistsAsync(string organization, string name);

        Task<Project> AddAsync(Project project);

        Task<Project> SetAsync(Project project);

        Task<Project> RemoveAsync(Project project);
    }
}

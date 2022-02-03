/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data;

public interface IProjectTemplateRepository : IDocumentRepository<ProjectTemplate>
{
    // Task<bool> NameExistsAsync(string organization, string name);

    Task<ProjectTemplate> GetDefaultAsync(string organization);
}

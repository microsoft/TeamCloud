/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Git.Services;

public interface IRepositoryService
{
    Task<ProjectTemplate> UpdateProjectTemplateAsync(ProjectTemplate projectTemplate);

    Task<ComponentTemplate> GetComponentTemplateAsync(ProjectTemplate projectTemplate, string templateId);

    IAsyncEnumerable<ComponentTemplate> GetComponentTemplatesAsync(ProjectTemplate projectTemplate);
}

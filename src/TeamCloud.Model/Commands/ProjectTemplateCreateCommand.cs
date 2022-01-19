/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands;

public sealed class ProjectTemplateCreateCommand : CreateCommand<ProjectTemplate, ProjectTemplateCreateCommandResult>
{
    public ProjectTemplateCreateCommand(User user, ProjectTemplate payload)
        : base(user, payload)
    { }
}

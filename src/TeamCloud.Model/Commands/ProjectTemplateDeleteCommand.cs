/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands;

public sealed class ProjectTemplateDeleteCommand : DeleteCommand<ProjectTemplate, ProjectTemplateDeleteCommandResult>
{
    public ProjectTemplateDeleteCommand(User user, ProjectTemplate payload)
        : base(user, payload)
    { }
}

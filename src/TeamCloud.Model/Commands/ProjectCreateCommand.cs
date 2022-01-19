/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands;

public class ProjectCreateCommand : CreateCommand<Project, ProjectCreateCommandResult>
{
    public ProjectCreateCommand(User user, Project payload)
        : base(user, payload)
    { }
}

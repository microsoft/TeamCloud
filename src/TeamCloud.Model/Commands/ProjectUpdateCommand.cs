/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands;

public class ProjectUpdateCommand : UpdateCommand<Project, ProjectUpdateCommandResult>
{
    public ProjectUpdateCommand(User user, Project payload)
        : base(user, payload)
    { }
}

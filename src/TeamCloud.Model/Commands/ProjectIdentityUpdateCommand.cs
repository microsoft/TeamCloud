/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands;

public sealed class ProjectIdentityUpdateCommand : UpdateCommand<ProjectIdentity, ProjectIdentityUpdateCommandResult>
{
    public ProjectIdentityUpdateCommand(User user, ProjectIdentity payload)
        : base(user, payload)
    { }
}

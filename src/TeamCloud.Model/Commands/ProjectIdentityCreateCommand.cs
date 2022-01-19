/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands;

public sealed class ProjectIdentityCreateCommand : CreateCommand<ProjectIdentity, ProjectIdentityCreateCommandResult>
{
    public ProjectIdentityCreateCommand(User user, ProjectIdentity payload)
        : base(user, payload)
    { }
}

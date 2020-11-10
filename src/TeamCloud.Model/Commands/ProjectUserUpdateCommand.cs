/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProjectUserUpdateCommand : UpdateCommand<User, ProjectUserUpdateCommandResult>
    {
        public ProjectUserUpdateCommand(User user, User payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

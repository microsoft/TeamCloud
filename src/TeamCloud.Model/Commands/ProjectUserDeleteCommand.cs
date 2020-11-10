/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProjectUserDeleteCommand : DeleteCommand<User, ProjectUserDeleteCommandResult>
    {
        public ProjectUserDeleteCommand(User user, User payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

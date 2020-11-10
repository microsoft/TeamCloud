/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUserDeleteCommand : OrchestratorDeleteCommand<User, OrchestratorProjectUserDeleteCommandResult>
    {
        public OrchestratorProjectUserDeleteCommand(User user, User payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

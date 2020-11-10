/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUserCreateCommand : OrchestratorCreateCommand<User, OrchestratorProjectUserCreateCommandResult>
    {
        public OrchestratorProjectUserCreateCommand(User user, User payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

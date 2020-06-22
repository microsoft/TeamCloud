/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProjectUserDeleteCommand : OrchestratorCommand<User, OrchestratorProjectUserDeleteCommandResult, ProviderProjectUserDeleteCommand, Model.Data.User>
    {
        public OrchestratorProjectUserDeleteCommand(User user, User payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

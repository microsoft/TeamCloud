/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProjectUserUpdateCommand : OrchestratorCommand<User, OrchestratorProjectUserUpdateCommandResult, ProviderProjectUserUpdateCommand, Model.Data.User>
    {
        public OrchestratorProjectUserUpdateCommand(User user, User payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

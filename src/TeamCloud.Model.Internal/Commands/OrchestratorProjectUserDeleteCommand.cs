/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUserDeleteCommand : OrchestratorCommand<User, OrchestratorProjectUserDeleteCommandResult, ProviderProjectUserDeleteCommand>
    {
        public OrchestratorProjectUserDeleteCommand(User user, User payload, Guid projectId) : base(user, payload)
            => ProjectId = projectId;

        public override ProviderProjectUserDeleteCommand CreateProviderCommand()
            => new ProviderProjectUserDeleteCommand(this.User, this.Payload, this.ProjectId.Value, this.CommandId);
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUserUpdateCommand : OrchestratorCommand<User, OrchestratorProjectUserUpdateCommandResult, ProviderProjectUserUpdateCommand>
    {
        public OrchestratorProjectUserUpdateCommand(User user, User payload, Guid projectId) : base(user, payload)
            => ProjectId = projectId;

        public override ProviderProjectUserUpdateCommand CreateProviderCommand()
            => new ProviderProjectUserUpdateCommand(this.User, this.Payload, this.ProjectId.Value, this.CommandId);
    }
}

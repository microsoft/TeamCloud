/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUserCreateCommand : OrchestratorCommand<User, OrchestratorProjectUserCreateCommandResult, ProviderProjectUserCreateCommand>
    {

        public OrchestratorProjectUserCreateCommand(User user, User payload, Guid projectId) : base(user, payload)
            => ProjectId = projectId;

        public override ProviderProjectUserCreateCommand CreateProviderCommand()
            => new ProviderProjectUserCreateCommand(this.User, this.Payload, this.ProjectId.Value, this.CommandId);
    }
}

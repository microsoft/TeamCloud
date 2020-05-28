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
        public OrchestratorProjectUserCreateCommand(User user, User payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

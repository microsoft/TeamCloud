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
        public OrchestratorProjectUserDeleteCommand(User user, User payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProjectUserCreateCommand : OrchestratorCommand<User, OrchestratorProjectUserCreateCommandResult, ProviderProjectUserCreateCommand, Model.Data.User>
    {
        public OrchestratorProjectUserCreateCommand(Uri api, User user, User payload, string projectId) : base(api, user, payload)
            => ProjectId = projectId;
    }
}

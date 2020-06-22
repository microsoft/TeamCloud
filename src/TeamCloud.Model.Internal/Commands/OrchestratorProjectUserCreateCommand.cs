/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProjectUserCreateCommand : OrchestratorCommand<User, OrchestratorProjectUserCreateCommandResult, ProviderProjectUserCreateCommand, Model.Data.User>
    {
        public OrchestratorProjectUserCreateCommand(User user, User payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

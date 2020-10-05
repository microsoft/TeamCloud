/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUserDeleteCommand : OrchestratorCommand<UserDocument, OrchestratorProjectUserDeleteCommandResult, ProviderProjectUserDeleteCommand, Model.Data.User>
    {
        public OrchestratorProjectUserDeleteCommand(UserDocument user, UserDocument payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

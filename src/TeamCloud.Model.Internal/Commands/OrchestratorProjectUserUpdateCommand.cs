/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUserUpdateCommand : OrchestratorCommand<UserDocument, OrchestratorProjectUserUpdateCommandResult, ProviderProjectUserUpdateCommand, Model.Data.User>
    {
        public OrchestratorProjectUserUpdateCommand(UserDocument user, UserDocument payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

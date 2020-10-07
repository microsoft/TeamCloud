/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUserUpdateCommand : OrchestratorUpdateCommand<UserDocument, OrchestratorProjectUserUpdateCommandResult, ProviderProjectUserUpdateCommand, User>
    {
        public OrchestratorProjectUserUpdateCommand(UserDocument user, UserDocument payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

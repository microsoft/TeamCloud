/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUserCreateCommand : OrchestratorCommand<UserDocument, OrchestratorProjectUserCreateCommandResult, ProviderProjectUserCreateCommand, Model.Data.User>
    {
        public OrchestratorProjectUserCreateCommand(UserDocument user, UserDocument payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

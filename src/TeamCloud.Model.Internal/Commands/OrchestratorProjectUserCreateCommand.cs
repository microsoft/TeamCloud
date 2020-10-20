/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUserCreateCommand : OrchestratorCreateCommand<UserDocument, OrchestratorProjectUserCreateCommandResult, ProviderProjectUserCreateCommand, User>
    {
        public OrchestratorProjectUserCreateCommand(UserDocument user, UserDocument payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

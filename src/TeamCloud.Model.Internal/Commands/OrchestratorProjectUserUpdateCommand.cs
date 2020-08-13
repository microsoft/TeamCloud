/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProjectUserUpdateCommand : OrchestratorCommand<UserDocument, OrchestratorProjectUserUpdateCommandResult, ProviderProjectUserUpdateCommand, Model.Data.User>
    {
        public OrchestratorProjectUserUpdateCommand(UserDocument user, UserDocument payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

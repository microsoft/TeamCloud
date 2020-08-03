/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProjectUserDeleteCommand : OrchestratorCommand<UserDocument, OrchestratorProjectUserDeleteCommandResult, ProviderProjectUserDeleteCommand, Model.Data.User>
    {
        public OrchestratorProjectUserDeleteCommand(Uri api, UserDocument user, UserDocument payload, string projectId) : base(api, user, payload)
            => ProjectId = projectId;
    }
}

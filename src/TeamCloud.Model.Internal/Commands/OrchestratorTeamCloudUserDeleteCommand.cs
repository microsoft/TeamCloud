/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorTeamCloudUserDeleteCommand : OrchestratorCommand<UserDocument, OrchestratorTeamCloudUserDeleteCommandResult>
    {
        public OrchestratorTeamCloudUserDeleteCommand(Uri api, UserDocument user, UserDocument payload) : base(api, user, payload) { }
    }
}

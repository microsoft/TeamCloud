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
        public OrchestratorTeamCloudUserDeleteCommand(Uri baseApi, UserDocument user, UserDocument payload) : base(baseApi, user, payload) { }
    }
}

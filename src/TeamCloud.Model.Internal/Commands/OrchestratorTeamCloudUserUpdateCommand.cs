/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorTeamCloudUserUpdateCommand : OrchestratorCommand<UserDocument, OrchestratorTeamCloudUserUpdateCommandResult>
    {
        public OrchestratorTeamCloudUserUpdateCommand(Uri api, UserDocument user, UserDocument payload) : base(api, user, payload) { }
    }
}

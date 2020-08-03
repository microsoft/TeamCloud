/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorTeamCloudUserCreateCommand : OrchestratorCommand<UserDocument, OrchestratorTeamCloudUserCreateCommandResult>
    {
        public OrchestratorTeamCloudUserCreateCommand(Uri api, UserDocument user, UserDocument payload) : base(api, user, payload) { }
    }
}

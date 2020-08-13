/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProjectUpdateCommand : OrchestratorCommand<ProjectDocument, OrchestratorProjectUpdateCommandResult, ProviderProjectUpdateCommand, Model.Data.Project>
    {
        public OrchestratorProjectUpdateCommand(UserDocument user, ProjectDocument payload) : base(user, payload) { }
    }
}

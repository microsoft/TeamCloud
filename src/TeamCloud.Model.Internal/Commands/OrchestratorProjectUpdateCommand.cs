/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUpdateCommand : OrchestratorCommand<ProjectDocument, OrchestratorProjectUpdateCommandResult, ProviderProjectUpdateCommand, Model.Data.Project>
    {
        public OrchestratorProjectUpdateCommand(UserDocument user, ProjectDocument payload) : base(user, payload) { }
    }
}

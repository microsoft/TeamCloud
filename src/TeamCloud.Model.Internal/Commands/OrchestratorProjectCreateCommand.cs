/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProjectCreateCommand : OrchestratorCommand<ProjectDocument, OrchestratorProjectCreateCommandResult, ProviderProjectCreateCommand, Model.Data.Project>
    {
        public OrchestratorProjectCreateCommand(UserDocument user, ProjectDocument payload) : base(user, payload)
        { }
    }
}

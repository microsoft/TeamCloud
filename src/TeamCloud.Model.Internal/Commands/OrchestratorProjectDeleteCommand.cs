/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProjectDeleteCommand : OrchestratorCommand<ProjectDocument, OrchestratorProjectDeleteCommandResult, ProviderProjectDeleteCommand, Model.Data.Project>
    {
        public OrchestratorProjectDeleteCommand(Uri api, UserDocument user, ProjectDocument payload) : base(api, user, payload) { }
    }
}

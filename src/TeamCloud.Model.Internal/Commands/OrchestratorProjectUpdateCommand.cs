/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProjectUpdateCommand : OrchestratorCommand<Project, OrchestratorProjectUpdateCommandResult, ProviderProjectUpdateCommand, Model.Data.Project>
    {
        public OrchestratorProjectUpdateCommand(Uri api, User user, Project payload) : base(api, user, payload) { }
    }
}

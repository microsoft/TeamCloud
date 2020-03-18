/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUpdateCommand : OrchestratorCommand<Project, OrchestratorProjectUpdateCommandResult, ProviderProjectUpdateCommand>
    {
        public OrchestratorProjectUpdateCommand(User user, Project payload) : base(user, payload) { }
    }
}

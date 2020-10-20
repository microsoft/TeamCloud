/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUpdateCommand : OrchestratorUpdateCommand<ProjectDocument, OrchestratorProjectUpdateCommandResult, ProviderProjectUpdateCommand, Project>
    {
        public OrchestratorProjectUpdateCommand(UserDocument user, ProjectDocument payload) : base(user, payload) { }
    }
}

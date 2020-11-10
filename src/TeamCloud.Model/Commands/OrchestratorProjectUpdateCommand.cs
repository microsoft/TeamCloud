/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectUpdateCommand : OrchestratorUpdateCommand<Project, OrchestratorProjectUpdateCommandResult>
    {
        public OrchestratorProjectUpdateCommand(User user, Project payload) : base(user, payload) { }
    }
}

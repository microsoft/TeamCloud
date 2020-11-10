/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProjectDeleteCommand : OrchestratorDeleteCommand<Project, OrchestratorProjectDeleteCommandResult>
    {
        public OrchestratorProjectDeleteCommand(User user, Project payload) : base(user, payload) { }
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProjectDestroyCommand : CustomCommand<Project, ProjectDeployCommandResult>
    {
        public ProjectDestroyCommand(User user, Project payload) : base(user, payload) { }
    }

}

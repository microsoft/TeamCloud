/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ProjectComponentCreateCommand : CreateCommand<Component, ProjectComponentCreateCommandResult>
    {
        public ProjectComponentCreateCommand(User user, Component payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

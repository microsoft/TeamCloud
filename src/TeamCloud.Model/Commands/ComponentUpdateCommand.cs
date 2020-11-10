/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ComponentUpdateCommand : UpdateCommand<Component, ComponentUpdateCommandResult>
    {
        public ComponentUpdateCommand(User user, Component payload, string projectId) : base(user, payload)
            => ProjectId = projectId;
    }
}

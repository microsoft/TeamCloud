/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ComponentMonitorCommand : CustomCommand<Component, ComponentMonitorCommandResult>
    {
        public ComponentMonitorCommand(User user, Component payload) : base(user, payload)
        { }
    }
}

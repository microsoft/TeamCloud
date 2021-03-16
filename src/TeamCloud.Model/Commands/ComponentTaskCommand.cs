/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ComponentTaskCommand : CustomCommand<ComponentTask, ComponentTaskCommandResult>
    {
        public ComponentTaskCommand(User user, ComponentTask payload) : base(user, payload)
        { }
    }
}

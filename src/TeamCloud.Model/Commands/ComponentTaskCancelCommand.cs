/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ComponentTaskCancelCommand : CustomCommand<ComponentTask, ComponentTaskCancelCommandResult>
    {
        public ComponentTaskCancelCommand(User user, ComponentTask payload) : base(user, payload)
        { }
    }
}

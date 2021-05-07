/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ScheduledTaskRunCommand : CustomCommand<ScheduledTask, ScheduledTaskRunCommandResult>
    {
        public ScheduledTaskRunCommand(User user, ScheduledTask payload) : base(user, payload)
        { }

    }
}

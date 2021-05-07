/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ScheduledTaskUpdateCommand : UpdateCommand<ScheduledTask, ScheduledTaskUpdateCommandResult>
    {
        public ScheduledTaskUpdateCommand(User user, ScheduledTask payload) : base(user, payload)
        { }

    }
}

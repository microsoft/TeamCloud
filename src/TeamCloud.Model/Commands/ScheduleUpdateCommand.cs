/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ScheduleUpdateCommand : UpdateCommand<Schedule, ScheduleUpdateCommandResult>
    {
        public ScheduleUpdateCommand(User user, Schedule payload) : base(user, payload)
        { }

    }
}

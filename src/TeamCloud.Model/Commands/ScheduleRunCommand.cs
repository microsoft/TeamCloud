/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands;

public sealed class ScheduleRunCommand : CustomCommand<Schedule, ScheduleRunCommandResult>
{
    public ScheduleRunCommand(User user, Schedule payload)
        : base(user, payload)
    { }
}

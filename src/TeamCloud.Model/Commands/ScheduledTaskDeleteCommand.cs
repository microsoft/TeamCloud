/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ScheduledTaskDeleteCommand : DeleteCommand<ScheduledTask, ScheduledTaskDeleteCommandResult>
    {
        public ScheduledTaskDeleteCommand(User user, ScheduledTask payload) : base(user, payload)
        { }
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class BroadcastCommandResultCommand<TCommandResult> : CustomCommand<TCommandResult, BroadcastCommandResultCommandResult>
        where TCommandResult : class, ICommandResult, new()
    {
        public BroadcastCommandResultCommand(User user, TCommandResult payload) : base(user, payload)
        { }
    }
}

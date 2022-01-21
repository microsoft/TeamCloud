/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Messaging;

namespace TeamCloud.Model.Commands;

public sealed class NotificationSendMailCommand<TMessage> : CustomCommand<TMessage, NotificationSendMailCommandResult<TMessage>>
    where TMessage : NotificationMessage, new()
{
    public NotificationSendMailCommand(User user, TMessage payload)
        : base(user, payload)
    { }
}

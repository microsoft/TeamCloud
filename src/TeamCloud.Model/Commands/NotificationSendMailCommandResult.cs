/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Messaging;

namespace TeamCloud.Model.Commands
{
    public sealed class NotificationSendMailCommandResult<TMessage> : CommandResult<TMessage>
        where TMessage : NotificationMessage, new()
    {
    }
}

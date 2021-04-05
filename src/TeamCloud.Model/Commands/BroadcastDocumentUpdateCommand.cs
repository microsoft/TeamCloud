/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Commands
{
    public sealed class BroadcastDocumentUpdateCommand<TPayload> : UpdateCommand<TPayload, BroadcastDocumentUpdateCommandResult>
        where TPayload : class, IContainerDocument, new()
    {
        public BroadcastDocumentUpdateCommand(User user, TPayload payload) : base(user, payload, Guid.TryParse(payload?.ETag?.Trim('"'), out var commandId) ? commandId : default)
        { }
    }
}

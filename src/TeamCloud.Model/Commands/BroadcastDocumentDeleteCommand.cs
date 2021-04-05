using System;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;

namespace TeamCloud.Model.Commands
{
    public sealed class BroadcastDocumentDeleteCommand<TPayload> : DeleteCommand<TPayload, BroadcastDocumentDeleteCommandResult>
        where TPayload : class, IContainerDocument, new()
    {
        public BroadcastDocumentDeleteCommand(User user, TPayload payload) : base(user, payload, Guid.TryParse(payload?.ETag?.Trim('"'), out var commandId) ? commandId : default)
        { }
    }
}

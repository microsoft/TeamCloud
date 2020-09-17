/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using Microsoft.Azure.EventGrid.Models;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderEventCommand : ProviderCommand<EventGridEvent, ProviderEventCommandResult>
    {
        public ProviderEventCommand(User user, EventGridEvent payload)
            : base(user, payload, Guid.TryParse(payload?.Id, out var commandId) ? commandId : default(Guid?))
        { }
    }
}

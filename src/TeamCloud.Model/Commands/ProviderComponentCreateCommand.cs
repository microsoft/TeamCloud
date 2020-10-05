/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderComponentCreateCommand : ProviderCommand<Component, ProviderComponentCreateCommandResult>
    {
        public ProviderComponentCreateCommand(User user, Component payload, string projectId, Guid? commandId = null) : base(user, payload, commandId)
            => ProjectId = projectId;
    }
}

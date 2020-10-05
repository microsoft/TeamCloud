/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderComponentDeleteCommand : ProviderCommand<Component, ProviderComponentDeleteCommandResult>
    {
        public ProviderComponentDeleteCommand(User user, Component payload, string projectId, Guid? commandId = null) : base(user, payload, commandId)
            => ProjectId = projectId;
    }
}

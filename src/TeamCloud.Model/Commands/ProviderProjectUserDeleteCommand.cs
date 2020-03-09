/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderProjectUserDeleteCommand : ProviderCommand<User, ProviderProjectUserDeleteCommandResult>
    {
        public ProviderProjectUserDeleteCommand(User user, User payload, Guid projectId, Guid? commandId = null) : base(user, payload, commandId)
            => this.ProjectId = projectId;
    }
}

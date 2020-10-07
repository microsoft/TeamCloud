/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderProjectUserUpdateCommand : ProviderUpdateCommand<User, ProviderProjectUserUpdateCommandResult>
    {
        public ProviderProjectUserUpdateCommand(User user, User payload, string projectId, Guid? commandId = null) : base(user, payload, commandId)
            => this.ProjectId = projectId;
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderProjectUserUpdateCommand : ProviderCommand<User, ProviderProjectUserUpdateCommandResult>
    {
        public ProviderProjectUserUpdateCommand(Uri api, User user, User payload, string projectId, Guid? commandId = null) : base(api, user, payload, commandId)
            => this.ProjectId = projectId;
    }
}

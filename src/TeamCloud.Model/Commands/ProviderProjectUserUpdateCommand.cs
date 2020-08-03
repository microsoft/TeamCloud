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
        public ProviderProjectUserUpdateCommand(Uri baseApi, User user, User payload, string projectId, Guid? commandId = null) : base(baseApi, user, payload, commandId)
            => this.ProjectId = projectId;
    }
}

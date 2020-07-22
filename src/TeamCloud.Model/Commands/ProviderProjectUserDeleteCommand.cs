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
        public ProviderProjectUserDeleteCommand(Uri api, User user, User payload, string projectId, Guid? commandId = null) : base(api, user, payload, commandId)
            => ProjectId = projectId;
    }
}

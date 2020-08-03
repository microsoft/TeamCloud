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
        public ProviderProjectUserDeleteCommand(Uri baseApi, User user, User payload, string projectId, Guid? commandId = null) : base(baseApi, user, payload, commandId)
            => ProjectId = projectId;
    }
}

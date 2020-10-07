/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderProjectUserCreateCommand : ProviderCreateCommand<User, ProviderProjectUserCreateCommandResult>
    {
        public ProviderProjectUserCreateCommand(User user, User payload, string projectId, Guid? commandId = default) : base(user, payload, commandId)
            => ProjectId = projectId;
    }
}

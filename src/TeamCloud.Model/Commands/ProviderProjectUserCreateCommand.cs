/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderProjectUserCreateCommand : ProviderCommand<User, ProviderProjectUserCreateCommandResult>
    {
        public ProviderProjectUserCreateCommand(Uri baseApi, User user, User payload, string projectId, Guid? commandId = default) : base(baseApi, user, payload, commandId)
            => ProjectId = projectId;
    }
}

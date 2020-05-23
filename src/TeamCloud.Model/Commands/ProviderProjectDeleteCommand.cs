﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderProjectDeleteCommand : ProviderCommand<Project, ProviderProjectDeleteCommandResult>
    {
        public ProviderProjectDeleteCommand(User user, Project payload, Guid? commandId = null) : base(user, payload, commandId)
            => ProjectId = Guid.TryParse(payload?.Id, out var projectId) ? projectId : throw new ArgumentNullException(nameof(payload));
    }
}

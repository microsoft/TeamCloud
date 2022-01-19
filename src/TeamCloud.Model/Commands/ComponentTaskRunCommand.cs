/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands;

public sealed class ComponentTaskRunCommand : CustomCommand<ComponentTask, ComponentTaskRunCommandResult>
{
    public ComponentTaskRunCommand(User user, ComponentTask payload)
        : base(user, payload, Guid.TryParse(payload?.Id, out Guid id) ? id : (Guid?)null)
    { }
}

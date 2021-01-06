/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ComponentDeleteCommand : DeleteCommand<Component, ComponentDeleteCommandResult>
    {
        public ComponentDeleteCommand(User user, Component payload) : base(user, payload)
            => ProjectId = payload?.ProjectId ?? throw new System.ArgumentNullException(nameof(payload));
    }
}

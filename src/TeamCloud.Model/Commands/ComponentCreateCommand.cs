/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ComponentCreateCommand : CreateCommand<Component, ComponentCreateCommandResult>
    {
        public ComponentCreateCommand(User user, Component payload) : base(user, payload)
            => ProjectId = payload?.ProjectId ?? throw new System.ArgumentNullException(nameof(payload));
    }
}

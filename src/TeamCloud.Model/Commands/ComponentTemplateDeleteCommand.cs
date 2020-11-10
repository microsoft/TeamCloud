/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class ComponentTemplateDeleteCommand : DeleteCommand<ComponentTemplate, ComponentTemplateDeleteCommandResult>
    {
        public ComponentTemplateDeleteCommand(User user, ComponentTemplate payload) : base(user, payload) { }
    }
}

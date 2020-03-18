/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class ProviderProjectUpdateCommand : ProviderCommand<Project, ProviderProjectUpdateCommandResult>
    {
        public ProviderProjectUpdateCommand(User user, Project payload, Guid? commandId = null) : base(user, payload, commandId)
            => ProjectId = payload?.Id ?? throw new ArgumentNullException(nameof(payload));

        public override Guid? ProjectId
        {
            get => (base.Payload as Project)?.Id ?? base.ProjectId;
            set => base.ProjectId = value;
        }

    }
}

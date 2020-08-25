/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProviderDataUpdateCommand : OrchestratorCommand<ProviderDataDocument, OrchestratorProviderDataUpdateCommandResult>
    {
        public OrchestratorProviderDataUpdateCommand(UserDocument user, ProviderDataDocument payload) : base(user, payload)
        {
        }
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProviderCreateCommand : OrchestratorCommand<ProviderDocument, OrchestratorProviderCreateCommandResult>
    {
        public OrchestratorProviderCreateCommand(UserDocument user, ProviderDocument payload) : base(user, payload) { }
    }
}

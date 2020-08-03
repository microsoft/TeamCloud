/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProviderCreateCommand : OrchestratorCommand<ProviderDocument, OrchestratorProviderCreateCommandResult>
    {
        public OrchestratorProviderCreateCommand(Uri api, UserDocument user, ProviderDocument payload) : base(api, user, payload) { }
    }
}

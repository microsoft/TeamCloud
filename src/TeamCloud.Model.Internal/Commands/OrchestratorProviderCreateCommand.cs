/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProviderCreateCommand : OrchestratorCommand<Provider, OrchestratorProviderCreateCommandResult>
    {
        public OrchestratorProviderCreateCommand(Uri api, User user, Provider payload) : base(api, user, payload) { }
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProviderCreateCommand : OrchestratorCommand<Provider, OrchestratorProviderCreateCommandResult>
    {
        public OrchestratorProviderCreateCommand(User user, Provider payload) : base(user, payload) { }
    }
}

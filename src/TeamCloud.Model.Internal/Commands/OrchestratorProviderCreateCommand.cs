/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProviderCreateCommand : OrchestratorCommand<Provider, OrchestratorProviderCreateCommandResult>
    {
        public OrchestratorProviderCreateCommand(User user, Provider payload) : base(user, payload) { }
    }
}

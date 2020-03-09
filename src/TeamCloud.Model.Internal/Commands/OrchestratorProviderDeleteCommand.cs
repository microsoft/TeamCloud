/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProviderDeleteCommand : OrchestratorCommand<Provider, OrchestratorProviderDeleteCommandResult>
    {
        public OrchestratorProviderDeleteCommand(User user, Provider payload) : base(user, payload) { }
    }
}

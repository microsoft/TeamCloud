/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProviderDeleteCommand : OrchestratorCommand<Provider, OrchestratorProviderDeleteCommandResult>
    {
        public OrchestratorProviderDeleteCommand(User user, Provider payload) : base(user, payload) { }
    }
}

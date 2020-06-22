/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProviderUpdateCommand : OrchestratorCommand<Provider, OrchestratorProviderUpdateCommandResult>
    {
        public OrchestratorProviderUpdateCommand(User user, Provider payload) : base(user, payload) { }
    }
}

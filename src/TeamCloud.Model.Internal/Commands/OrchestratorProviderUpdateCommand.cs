/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProviderUpdateCommand : OrchestratorCommand<Provider, OrchestratorProviderUpdateCommandResult>
    {
        public OrchestratorProviderUpdateCommand(User user, Provider payload) : base(user, payload) { }
    }
}

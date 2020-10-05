/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProviderDeleteCommand : OrchestratorCommand<ProviderDocument, OrchestratorProviderDeleteCommandResult>
    {
        public OrchestratorProviderDeleteCommand(UserDocument user, ProviderDocument payload) : base(user, payload) { }
    }
}

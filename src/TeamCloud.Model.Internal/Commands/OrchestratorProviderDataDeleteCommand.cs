/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProviderDataDeleteCommand : OrchestratorCommand<ProviderDataDocument, OrchestratorProviderDataDeleteCommandResult>
    {
        public OrchestratorProviderDataDeleteCommand(UserDocument user, ProviderDataDocument payload) : base(user, payload)
        {
        }
    }
}

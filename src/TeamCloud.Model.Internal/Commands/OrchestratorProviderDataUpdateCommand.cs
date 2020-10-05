/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProviderDataUpdateCommand : OrchestratorCommand<ProviderDataDocument, OrchestratorProviderDataUpdateCommandResult>
    {
        public OrchestratorProviderDataUpdateCommand(UserDocument user, ProviderDataDocument payload) : base(user, payload)
        {
        }
    }
}

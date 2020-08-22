/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorProviderDataCreateCommand : OrchestratorCommand<ProviderDataDocument, OrchestratorProviderDataCreateCommandResult>
    {
        public OrchestratorProviderDataCreateCommand(UserDocument user, ProviderDataDocument payload) : base(user, payload)
        {
        }
    }

}

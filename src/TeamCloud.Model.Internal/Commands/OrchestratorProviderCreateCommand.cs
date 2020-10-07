/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public class OrchestratorProviderCreateCommand : OrchestratorCreateCommand<ProviderDocument, OrchestratorProviderCreateCommandResult>
    {
        public OrchestratorProviderCreateCommand(UserDocument user, ProviderDocument payload) : base(user, payload) { }
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProviderUpdateCommand : OrchestratorCommand<ProviderDocument, OrchestratorProviderUpdateCommandResult>
    {
        public OrchestratorProviderUpdateCommand(UserDocument user, ProviderDocument payload) : base(user, payload) { }
    }
}

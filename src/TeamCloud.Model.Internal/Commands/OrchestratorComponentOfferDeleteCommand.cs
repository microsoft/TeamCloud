/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentOfferDeleteCommand : OrchestratorCommand<ComponentOfferDocument, OrchestratorComponentOfferDeleteCommandResult>
    {
        public OrchestratorComponentOfferDeleteCommand(UserDocument user, ComponentOfferDocument payload) : base(user, payload) { }
    }
}

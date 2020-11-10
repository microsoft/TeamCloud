/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentOfferDeleteCommand : OrchestratorDeleteCommand<ComponentOffer, OrchestratorComponentOfferDeleteCommandResult>
    {
        public OrchestratorComponentOfferDeleteCommand(User user, ComponentOffer payload) : base(user, payload) { }
    }
}

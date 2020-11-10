/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentOfferCreateCommand : OrchestratorCreateCommand<ComponentOffer, OrchestratorComponentOfferCreateCommandResult>
    {
        public OrchestratorComponentOfferCreateCommand(User user, ComponentOffer payload) : base(user, payload) { }
    }
}

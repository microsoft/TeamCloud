/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentOfferUpdateCommand : OrchestratorUpdateCommand<ComponentOffer, OrchestratorComponentOfferUpdateCommandResult>
    {
        public OrchestratorComponentOfferUpdateCommand(User user, ComponentOffer payload) : base(user, payload) { }
    }
}

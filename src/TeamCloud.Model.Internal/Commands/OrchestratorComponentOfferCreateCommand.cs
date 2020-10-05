/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentOfferCreateCommand : OrchestratorCommand<ComponentOfferDocument, OrchestratorComponentOfferCreateCommandResult>
    {
        public OrchestratorComponentOfferCreateCommand(UserDocument user, ComponentOfferDocument payload) : base(user, payload) { }
    }
}

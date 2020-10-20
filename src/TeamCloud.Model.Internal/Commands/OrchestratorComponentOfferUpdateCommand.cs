/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using TeamCloud.Model.Data;

namespace TeamCloud.Model.Commands
{
    public sealed class OrchestratorComponentOfferUpdateCommand : OrchestratorUpdateCommand<ComponentOfferDocument, OrchestratorComponentOfferUpdateCommandResult>
    {
        public OrchestratorComponentOfferUpdateCommand(UserDocument user, ComponentOfferDocument payload) : base(user, payload) { }
    }
}

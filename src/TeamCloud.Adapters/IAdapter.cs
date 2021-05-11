/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;
using TeamCloud.Model.Handlers;

namespace TeamCloud.Adapters
{
    public interface IAdapter
    {
        IEnumerable<ICommandHandler> GetCommandHandlers();

        bool Supports(DeploymentScope deploymentScope);

        Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope);
    }
}

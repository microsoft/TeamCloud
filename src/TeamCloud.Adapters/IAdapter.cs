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
        DeploymentScopeType Type { get; }

        string DisplayName { get; }

        Task<string> GetInputDataSchemaAsync();

        Task<string> GetInputFormSchemaAsync();

        IEnumerable<ICommandHandler> GetCommandHandlers();

        Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope);
    }
}

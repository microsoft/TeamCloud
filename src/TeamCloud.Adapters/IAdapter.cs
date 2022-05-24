/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using TeamCloud.Model.Commands.Core;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters;

public interface IAdapter
{
    DeploymentScopeType Type { get; }

    string DisplayName { get; }

    IEnumerable<ComponentType> ComponentTypes { get; }

    Task<string> GetInputDataSchemaAsync();

    Task<string> GetInputFormSchemaAsync();

    Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope);

    Task<Component> CreateComponentAsync(Component component, User contextUser, IAsyncCollector<ICommand> commandQueue);

    Task<Component> UpdateComponentAsync(Component component, User contextUser, IAsyncCollector<ICommand> commandQueue);

    Task<Component> DeleteComponentAsync(Component component, User contextUser, IAsyncCollector<ICommand> commandQueue);
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Microsoft.Graph;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters;

public interface IAdapterIdentity : IAdapter
{
    Task<AzureServicePrincipal> GetServiceIdentityAsync(DeploymentScope deploymentScope, bool withPassword = false);

    Task<AzureServicePrincipal> GetServiceIdentityAsync(Component component, bool withPassword = false);
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Net;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters
{
    public interface IAdapter
    {
        DeploymentScopeType Type { get; }

        string DisplayName { get; }

        Task<string> GetInputDataSchemaAsync();

        Task<string> GetInputFormSchemaAsync();

        Task<bool> IsAuthorizedAsync(DeploymentScope deploymentScope);

        Task<Component> CreateComponentAsync(Component component);

        Task<Component> UpdateComponentAsync(Component component);

        Task<Component> DeleteComponentAsync(Component component);

        Task<NetworkCredential> GetServiceCredentialAsync(Component component);
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data
{
    public interface IDeploymentScopeRepository
    {
        Task<DeploymentScopeDocument> GetAsync(string id);

        IAsyncEnumerable<DeploymentScopeDocument> ListAsync();

        Task<DeploymentScopeDocument> GetDefaultAsync();

        Task<DeploymentScopeDocument> AddAsync(DeploymentScopeDocument deploymentScope);

        Task<DeploymentScopeDocument> SetAsync(DeploymentScopeDocument deploymentScope);

        Task<DeploymentScopeDocument> RemoveAsync(DeploymentScopeDocument deploymentScope);
    }
}

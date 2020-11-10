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
        Task<DeploymentScope> GetAsync(string id);

        IAsyncEnumerable<DeploymentScope> ListAsync();

        Task<DeploymentScope> GetDefaultAsync();

        Task<DeploymentScope> AddAsync(DeploymentScope deploymentScope);

        Task<DeploymentScope> SetAsync(DeploymentScope deploymentScope);

        Task<DeploymentScope> RemoveAsync(DeploymentScope deploymentScope);
    }
}

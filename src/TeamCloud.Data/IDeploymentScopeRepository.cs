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
        Task<DeploymentScope> GetAsync(string organization, string id);

        IAsyncEnumerable<DeploymentScope> ListAsync(string organization);

        Task<DeploymentScope> GetDefaultAsync(string organization);

        Task<DeploymentScope> AddAsync(DeploymentScope deploymentScope);

        Task<DeploymentScope> SetAsync(DeploymentScope deploymentScope);

        Task<DeploymentScope> RemoveAsync(DeploymentScope deploymentScope);
    }
}

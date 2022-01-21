/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data;

public interface IDeploymentScopeRepository : IDocumentRepository<DeploymentScope>
{
    Task<string> ResolveIdAsync(string tenant, string identifier);
    Task<DeploymentScope> GetDefaultAsync(string organization);
}

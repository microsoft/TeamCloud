/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Data;

public interface IDeploymentScopeRepository : IDocumentRepository<DeploymentScope>
{
    Task<bool> NameExistsAsync(string organization, string name);

    Task<string> ResolveIdAsync(string organization, string identifier);

    Task<DeploymentScope> GetDefaultAsync(string organization);
}

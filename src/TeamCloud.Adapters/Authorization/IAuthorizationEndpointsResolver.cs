/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters.Authorization;

public interface IAuthorizationEndpointsResolver
{
    Task<IAuthorizationEndpoints> GetAuthorizationEndpointsAsync(DeploymentScope deploymentScope);
}

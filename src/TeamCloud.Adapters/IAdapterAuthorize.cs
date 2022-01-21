/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Azure.Directory;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters;

public interface IAdapterAuthorize : IAdapter
{
    Task<AzureServicePrincipal> ResolvePrincipalAsync(DeploymentScope deploymentScope, HttpRequest request);

    Task<IActionResult> HandleAuthorizeAsync(DeploymentScope deploymentScope, HttpRequest request, IAuthorizationEndpoints authorizationEndpoints);

    Task<IActionResult> HandleCallbackAsync(DeploymentScope deploymentScope, HttpRequest request, IAuthorizationEndpoints authorizationEndpoints);
}

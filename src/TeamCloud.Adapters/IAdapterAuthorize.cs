/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TeamCloud.Adapters.Authorization;
using TeamCloud.Model.Data;

namespace TeamCloud.Adapters
{
    public interface IAdapterAuthorize : IAdapter
    {
        Task CreateSessionAsync(DeploymentScope deploymentScope);

        Task<IActionResult> HandleAuthorizeAsync(DeploymentScope deploymentScope, HttpRequestMessage requestMessage, IAuthorizationEndpoints authorizationEndpoints);

        Task<IActionResult> HandleCallbackAsync(DeploymentScope deploymentScope, HttpRequestMessage requestMessage, IAuthorizationEndpoints authorizationEndpoints);
    }
}

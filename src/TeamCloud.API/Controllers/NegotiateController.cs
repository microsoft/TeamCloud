/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR.Management;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Controllers.Core;
using TeamCloud.API.Options;
using TeamCloud.Model;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{organizationId:organizationId}/projects/{projectId:projectId}/negotiate")]
    public class NegotiateController : TeamCloudController
    {
        private readonly IServiceManager _serviceManager;

        public NegotiateController(IAzureSignalROptions azureSignalROptions)
        {
            if (azureSignalROptions is null)
                throw new ArgumentNullException(nameof(azureSignalROptions));

            _serviceManager = new ServiceManagerBuilder()
                .WithOptions(o => o.ConnectionString = azureSignalROptions.ConnectionString)
                .Build();
        }

        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "NegotiateSignalR", Summary = "Negotiates the SignalR connection.")]
        public Task<IActionResult> Index() => ExecuteAsync<TeamCloudProjectContext>(context =>
        {
            var hub = context.Project.GetHubName();
            var url = _serviceManager.GetClientEndpoint(hub);
            var token = _serviceManager.GenerateClientAccessToken(hub, context.ContextUser.Id);

            return Task.FromResult<IActionResult>(new JsonResult(new Dictionary<string, string>()
            {
                { "url", url },
                { "accessToken", token }
            }));
        });

    }
}

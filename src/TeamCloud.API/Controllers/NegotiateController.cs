/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.SignalR.Management;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Controllers.Core;
using TeamCloud.API.Options;
using TeamCloud.Model;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{organizationId:organizationId}/projects/{projectId:projectId}/negotiate")]
    public class NegotiateController : TeamCloudController
    {
        private readonly ServiceManager _serviceManager;

        public NegotiateController(IAzureSignalROptions azureSignalROptions)
        {
            if (azureSignalROptions is null)
                throw new ArgumentNullException(nameof(azureSignalROptions));

            _serviceManager = new ServiceManagerBuilder()
                .WithOptions(o => o.ConnectionString = azureSignalROptions.ConnectionString)
                .BuildServiceManager();
        }

        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "NegotiateSignalR", Summary = "Negotiates the SignalR connection.")]
        public Task<IActionResult> Index() => WithContextAsync<Project>(async (contextUser, project) =>
        {
            var hub = project.GetHubName();

            var serviceHubContext = await _serviceManager
                .CreateHubContextAsync(hub, CancellationToken.None)
                .ConfigureAwait(false);

            var negotiationResponse = await serviceHubContext
                .NegotiateAsync(new NegotiationOptions { UserId = contextUser.Id })
                .ConfigureAwait(false);

            var url = negotiationResponse.Url;
            var token = negotiationResponse.AccessToken;

            return new JsonResult(new Dictionary<string, string>()
            {
                { "url", url },
                { "accessToken", token }
            });
        });

    }
}

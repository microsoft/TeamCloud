//
//   Copyright (c) Microsoft Corporation.
//   Licensed under the MIT License.
//

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Controllers.Core;
using TeamCloud.API.Data.Results;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers;

[ApiController]
[Produces("application/json")]
public class OrganizationActionController : TeamCloudController
{
    [HttpPost("orgs/{organizationId:organizationId}/action/UpdatePortal")]
    [Authorize(Policy = AuthPolicies.OrganizationOwner)]
    [Consumes("application/json")]
    [SwaggerOperation(OperationId = "UpdatePortal", Summary = "Updates the custom portal of the organization")]
    [SwaggerResponse(StatusCodes.Status200OK, "The custom organization portal was successfully updated.", typeof(DataResult<Organization>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "The organization does not exist.", typeof(ErrorResult))]
    public Task<IActionResult> UpdatePortal() => WithContextAsync<Organization>(async (contextUser, organization) =>
    {
        var command = new OrganizationPortalUpdateCommand(contextUser, organization);

        return await Orchestrator
            .InvokeAndReturnActionResultAsync(command, Request)
            .ConfigureAwait(false);
    });

}

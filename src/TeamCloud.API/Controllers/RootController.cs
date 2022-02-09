/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Data;
using TeamCloud.API.Data.Results;
using TeamCloud.Configuration.Options;

namespace TeamCloud.API.Controllers;

[ApiController]
[Route("/")]
[Produces("application/json")]
public class RootController : ControllerBase
{
    private readonly ITeamCloudOptions teamCloudOptions;

    public RootController(ITeamCloudOptions teamCloudOptions)
    {
        this.teamCloudOptions = teamCloudOptions ?? throw new System.ArgumentNullException(nameof(teamCloudOptions));
    }

    [HttpGet]
    [AllowAnonymous]
    [SwaggerOperation(OperationId = "GetInfo", Summary = "Gets information about this TeamCloud deployment.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Returns information about this TeamCloud deployment.", typeof(DataResult<TeamCloudInformation>))]
    public IActionResult Get()
    {
        var info = new TeamCloudInformation
        {
            Version = teamCloudOptions.Version
        };

        return DataResult<TeamCloudInformation>
            .Ok(info)
            .ToActionResult();
    }
}

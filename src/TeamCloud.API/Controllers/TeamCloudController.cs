/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Data;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/config")]
    public class TeamCloudController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly ITeamCloudRepositoryReadOnly teamCloudRepository;
        readonly IProjectTypesRepositoryReadOnly projectTypesRepository;

        public TeamCloudController(UserService userService, Orchestrator orchestrator, ITeamCloudRepositoryReadOnly teamCloudRepository, IProjectTypesRepositoryReadOnly projectTypesRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
            this.projectTypesRepository = projectTypesRepository ?? throw new ArgumentNullException(nameof(projectTypesRepository));
        }

        private User CurrentUser => new User()
        {
            Id = userService.CurrentUserId,
            Role = UserRoles.Project.Owner
        };


        [HttpGet]
        [Authorize(Policy = "admin")]
        [Produces("application/json", "application/x-yaml")]
        [SwaggerOperation(OperationId = "GetTeamCloudConfiguration", Summary = "Gets the TeamCloud instance's TeamCloudConfiguration.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the TeamCloud instance's TeamCloudConfiguration.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            var projectTypes = await projectTypesRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            var config = new TeamCloudConfiguration
            {
                ProjectTypes = projectTypes,
                Providers = teamCloudInstance.Providers,
                Users = teamCloudInstance.Users,
                Tags = teamCloudInstance.Tags,
                Properties = teamCloudInstance.Properties,
            };

            return DataResult<TeamCloudConfiguration>
                .Ok(config)
                .ActionResult();
        }


        [HttpPost]
        [Authorize(Policy = "admin")]
        [Consumes("application/x-yaml")]
        [Produces("application/json")]
        [SwaggerOperation(OperationId = "PostTeamCloudConfiguration", Summary = "Configures the TeamCloud instance.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts configuring the new TeamCloud instance. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The teamCloudConfiguraiton provided in the request body did not pass validation.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "The TeamCloud instance is already configured.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] TeamCloudConfiguration teamCloudConfiguraiton)
        {
            var validation = new TeamCloudConfigurationValidator().Validate(teamCloudConfiguraiton);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance != null)
                return ErrorResult
                    .Conflict("A TeamCloud Instance already existis.")
                    .ActionResult();

            var command = new TeamCloudCreateCommand(CurrentUser, teamCloudConfiguraiton);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shoudn't happen, but we need to decide to do when it does...");
        }
    }
}

/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/config")]
    [Authorize(Policy = "admin")]
    public class TeamCloudController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly ITeamCloudRepositoryReadOnly teamCloudRepository;

        public TeamCloudController(UserService userService, Orchestrator orchestrator, ITeamCloudRepositoryReadOnly teamCloudRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        private User CurrentUser => new User()
        {
            Id = userService.CurrentUserId,
            Role = UserRoles.Project.Owner
        };

        [HttpGet]
        [Produces("application/json", "application/x-yaml")]
        public async Task<IActionResult> Get()
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return new NotFoundResult();

            return new OkObjectResult(teamCloudInstance.Configuration);
        }

        [HttpPost]
        [Consumes("application/x-yaml")]
        public async Task<IActionResult> Post([FromBody] TeamCloudConfiguration teamCloudConfiguraiton)
        {
            if (teamCloudConfiguraiton is null)
                return new BadRequestObjectResult("Unable to parse teamcloud.yaml file.");

            try
            {
                new TeamCloudConfigurationValidator().ValidateAndThrow(teamCloudConfiguraiton);
            }
            catch (ValidationException validationEx)
            {
                return new BadRequestObjectResult(validationEx.Errors);
            }

            var command = new TeamCloudCreateCommand(CurrentUser, new TeamCloudInstance(teamCloudConfiguraiton));

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            return commandResult.ActionResult();
        }
    }
}

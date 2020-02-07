/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize(Policy = "admin")]
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
        [Produces("application/json", "application/x-yaml")]
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
        [Consumes("application/x-yaml")]
        public async Task<IActionResult> Post([FromBody] TeamCloudConfiguration teamCloudConfiguraiton)
        {
            var validation = new TeamCloudConfigurationValidator().Validate(teamCloudConfiguraiton);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
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

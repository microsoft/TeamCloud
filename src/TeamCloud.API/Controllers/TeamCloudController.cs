/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.Data;
using TeamCloud.Model;
using YamlDotNet;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/config")]
    [Authorize(Policy = "admin")]
    public class TeamCloudController : ControllerBase
    {
        private User currentUser = new User
        {
            Id = Guid.Parse("bc8a62dc-c327-4418-a004-77c85c3fb488"),
            Role = UserRoles.TeamCloud.Admin
        };

        readonly Orchestrator orchestrator;
        readonly ITeamCloudRepositoryReadOnly teamCloudRepository;

        public TeamCloudController(Orchestrator orchestrator, ITeamCloudRepositoryReadOnly teamCloudRepository)
        {
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        // GET: api/config
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            return teamCloudInstance is null
                ? (IActionResult)new NotFoundResult()
                : new OkObjectResult(teamCloudInstance.Configuration);
        }

        // POST: api/config
        [HttpPost]
        [Consumes("application/x-yaml")]
        public async Task<IActionResult> Post([FromBody] TeamCloudConfiguraiton teamCloudConfiguraiton)
        {
            (bool valid, string validationError) = teamCloudConfiguraiton.Validate();

            if (!valid)
            {
                return new BadRequestObjectResult(validationError);
            }

            var teamCloud = new TeamCloudInstance
            {
                Configuration = teamCloudConfiguraiton
            };

            var command = new TeamCloudCreateCommand(currentUser, teamCloud);

            var commandResult = await orchestrator
                .InvokeAsync<TeamCloudInstance>(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
            {
                return new AcceptedResult(statusUrl, commandResult);
            }
            else
            {
                return new OkObjectResult(commandResult);
            }

            /* TODO:
             *
             * - Change the input to a file upload
             * - This will be in the form of a yaml file (see: https://github.com/microsoft/TeamCloud/blob/master/docs/teamcloud.yaml)
             * - Possibly save (cache) the file in storage
             * - Parse the file into the TeamCloudConfiguraiton
             * - ...
             */
        }
    }
}

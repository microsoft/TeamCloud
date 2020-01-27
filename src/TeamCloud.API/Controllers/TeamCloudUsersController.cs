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

namespace TeamCloud.API
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Policy = "admin")]
    public class TeamCloudUsersController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly ITeamCloudRepositoryReadOnly teamCloudRepository;

        public TeamCloudUsersController(UserService userService, Orchestrator orchestrator, ITeamCloudRepositoryReadOnly teamCloudRepository)
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
        public async Task<IActionResult> Get()
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            var users = teamCloudInstance?.Users;

            return users is null
                ? (IActionResult)new NotFoundResult()
                : new OkObjectResult(users);
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> Get(Guid userId)
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            var user = teamCloudInstance?.Users?.FirstOrDefault(u => u.Id == userId);

            return user is null
                ? (IActionResult)new NotFoundResult()
                : new OkObjectResult(user);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserDefinition userDefinition)
        {
            if (userDefinition is null) return new BadRequestResult();

            var newUser = new User
            {
                Id = Guid.NewGuid(), // TODO: Get user id from graph using userDefinition.Email
                Role = userDefinition.Role, // TODO: validate
                Tags = userDefinition.Tags
            };

            var command = new TeamCloudUserCreateCommand(CurrentUser, newUser);

            var commandResult = await orchestrator
                .InvokeAsync<User>(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
            {
                return new AcceptedResult(statusUrl, commandResult);
            }
            else
            {
                return new OkObjectResult(commandResult);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] User user)
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            var oldUser = teamCloudInstance?.Users?.FirstOrDefault(u => u.Id == user.Id);

            if (oldUser is null) return new NotFoundResult();

            // TODO: start TeamCloudUserUpdateOrchestration and replace the code below (only the orchestrator can write to the database)

            return new OkObjectResult(user);
        }

        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> Delete(Guid userId)
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            var user = teamCloudInstance?.Users?.FirstOrDefault(u => u.Id == userId);

            if (user is null) return new NotFoundResult();

            // TODO: start TeamCloudUserDeleteOrchestration and replace the code below (only the orchestrator can write to the database)

            return new OkResult();
        }
    }
}

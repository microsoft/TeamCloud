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

            if (teamCloudInstance is null)
                return new NotFoundResult();

            var users = teamCloudInstance?.Users;

            if (users is null)
                return new NotFoundResult();

            return new OkObjectResult(users);
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> Get(Guid userId)
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return new NotFoundResult();

            var user = teamCloudInstance?.Users?.FirstOrDefault(u => u.Id == userId);

            if (user is null)
                return new NotFoundResult();

            return new OkObjectResult(user); ;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserDefinition userDefinition)
        {
            if (userDefinition is null)
                return new BadRequestResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return new NotFoundResult();

            var newUser = await userService
                .GetUserAsync(userDefinition)
                .ConfigureAwait(false);

            if (newUser is null)
                return new NotFoundResult();

            if (teamCloudInstance.Users.Contains(newUser))
                return new ConflictObjectResult("User already esists in this TeamCloud Instance.");

            var command = new TeamCloudUserCreateCommand(CurrentUser, newUser);

            var commandResult = await orchestrator
                .InvokeAsync<User>(command)
                .ConfigureAwait(false);

            return commandResult.ActionResult();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] User user)
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return new NotFoundResult();

            var oldUser = teamCloudInstance?.Users?.FirstOrDefault(u => u.Id == user.Id);

            if (oldUser is null)
                return new NotFoundObjectResult("User does not esists in this TeamCloud Instance.");

            var command = new TeamCloudUserUpdateCommand(CurrentUser, user);

            var commandResult = await orchestrator
                .InvokeAsync<User>(command)
                .ConfigureAwait(false);

            return commandResult.ActionResult();
        }

        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> Delete(Guid userId)
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return new NotFoundResult();

            var user = teamCloudInstance?.Users?.FirstOrDefault(u => u.Id == userId);

            if (user is null)
                return new NotFoundObjectResult("User does not esists in this TeamCloud Instance.");

            var command = new TeamCloudUserDeleteCommand(CurrentUser, user);

            var commandResult = await orchestrator
                .InvokeAsync<User>(command)
                .ConfigureAwait(false);

            return commandResult.ActionResult();
        }
    }
}

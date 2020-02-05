/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.API.Data;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

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
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            var users = teamCloudInstance?.Users ?? new List<User>();

            return DataResult<List<User>>
                .Ok(users)
                .ActionResult();
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> Get(Guid userId)
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            var user = teamCloudInstance?.Users?.FirstOrDefault(u => u.Id == userId);

            if (user is null)
                return ErrorResult
                    .NotFound($"A User with the ID '{userId}' could not be found in this TeamCloud Instance.")
                    .ActionResult();

            return DataResult<User>
                .Ok(user)
                .ActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserDefinition userDefinition)
        {
            var validation = new UserDefinitionValidator().Validate(userDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            var newUser = await userService
                .GetUserAsync(userDefinition)
                .ConfigureAwait(false);

            if (newUser is null)
                return ErrorResult
                    .NotFound($"A User with the Email '{userDefinition.Email}' could not be found.")
                    .ActionResult();

            if (teamCloudInstance.Users.Contains(newUser))
                return ErrorResult
                    .Conflict($"A User with the Email '{userDefinition.Email}' already exists on this TeamCloud Instance. Please try your request again with a unique email or call PUT to update the existing User.")
                    .ActionResult();

            var command = new TeamCloudUserCreateCommand(CurrentUser, newUser);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shoudn't happen, but we need to decide to do when it does...");
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] User user)
        {
            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            var oldUser = teamCloudInstance?.Users?.FirstOrDefault(u => u.Id == user.Id);

            if (oldUser is null)
                return ErrorResult
                    .NotFound($"A User with the ID '{oldUser.Id}' could not be found on this TeamCloud Instance.")
                    .ActionResult();

            var command = new TeamCloudUserUpdateCommand(CurrentUser, user);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shoudn't happen, but we need to decide to do when it does...");
        }

        [HttpDelete("{userId:guid}")]
        public async Task<IActionResult> Delete(Guid userId)
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            var user = teamCloudInstance?.Users?.FirstOrDefault(u => u.Id == userId);

            if (user is null)
                return ErrorResult
                    .NotFound($"A User with the ID '{userId}' could not be found on this TeamCloud Instance.")
                    .ActionResult();

            var command = new TeamCloudUserDeleteCommand(CurrentUser, user);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shoudn't happen, but we need to decide to do when it does...");
        }
    }
}

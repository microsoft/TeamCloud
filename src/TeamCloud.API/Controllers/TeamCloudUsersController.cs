/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeamCloud.Data;
using TeamCloud.Model;

namespace TeamCloud.API
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Policy = "admin")]
    public class TeamCloudUsersController : ControllerBase
    {
        // FIXME:
        private User currentUser = new User
        {
            Id = Guid.Parse("bc8a62dc-c327-4418-a004-77c85c3fb488"),
            Role = UserRoles.TeamCloud.Admin
        };

        readonly Orchestrator orchestrator;
        readonly ITeamCloudRepositoryReadOnly teamCloudRepository;

        public TeamCloudUsersController(Orchestrator orchestrator, ITeamCloudRepositoryReadOnly teamCloudRepository)
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

            var users = teamCloudInstance?.Users;

            return users is null
                ? (IActionResult)new NotFoundResult()
                : new OkObjectResult(users);
        }

        // GET: api/users/{userId}
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

        // POST: api/users
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

            var command = new TeamCloudUserCreateCommand(currentUser, newUser);

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

        // PUT: api/users
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

        // DELETE: api/users/{userId}
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

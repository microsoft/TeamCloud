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

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Authorize(Policy = "projectRead")]
    [Route("api/projects/{projectId:guid}/users")]
    public class ProjectUsersController : ControllerBase
    {
        // FIXME:
        private User currentUser = new User
        {
            Id = Guid.Parse("bc8a62dc-c327-4418-a004-77c85c3fb488"),
            Role = UserRoles.TeamCloud.Admin
        };

        readonly Orchestrator orchestrator;
        readonly IProjectsRepository projectsRepository;

        public Guid? ProjectId {
            get {
                var projectId = RouteData.Values.GetValueOrDefault(nameof(ProjectId), StringComparison.OrdinalIgnoreCase)?.ToString();

                return (string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId));
            }
        }

        public ProjectUsersController(Orchestrator orchestrator, IProjectsRepository projectsRepository)
        {
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        // GET: api/projects/{projectId}/users
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!ProjectId.HasValue) return new NotFoundResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            var users = project?.Users;

            return users is null
                ? (IActionResult)new NotFoundResult()
                : new OkObjectResult(users);
        }

        // GET: api/projects/{projectId}/users/{userId}
        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> Get(Guid userId)
        {
            if (!ProjectId.HasValue) return new NotFoundResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            var user = project?.Users?.FirstOrDefault(u => u.Id == userId);

            return user is null
                ? (IActionResult)new NotFoundResult()
                : new OkObjectResult(user);
        }

        // POST: api/projects/{projectId}/users
        [HttpPost]
        [Authorize(Policy = "projectCreate")]
        public async Task<IActionResult> Post([FromBody] UserDefinition userDefinition)
        {
            if (!ProjectId.HasValue) return new NotFoundResult();

            if (userDefinition is null) return new BadRequestResult();

            var newUser = new User
            {
                Id = Guid.NewGuid(), // TODO: Get user id from graph using userDefinition.Email
                Role = userDefinition.Role, // TODO: validate
                Tags = userDefinition.Tags
            };

            var command = new ProjectUserCreateCommand(currentUser, newUser, ProjectId.Value);

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

        // PUT: api/projects/{projectId}/users
        [HttpPut]
        [Authorize(Policy = "projectCreate")]
        public async Task<IActionResult> Put([FromBody] User user)
        {
            if (!ProjectId.HasValue) return new NotFoundResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            var oldUser = project?.Users?.FirstOrDefault(u => u.Id == user.Id);

            if (oldUser is null) return new NotFoundResult();

            // TODO: send ProjectUserUpdateCommand and replace the code below (only the orchestrator can write to the database)

            return new OkObjectResult(user);
        }

        // DELETE: api/projects/{projectId}/users/{userId}
        [HttpDelete("{userId:guid}")]
        [Authorize(Policy = "projectCreate")]
        public async Task<IActionResult> Delete(Guid userId)
        {
            if (!ProjectId.HasValue) return new NotFoundResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            var user = project?.Users?.FirstOrDefault(u => u.Id == userId);

            if (user is null) return new NotFoundResult();

            var command = new ProjectUserDeleteCommand(currentUser, user, ProjectId.Value);

            var commandResult = await orchestrator
                .InvokeAsync<Project>(command)
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
    }
}

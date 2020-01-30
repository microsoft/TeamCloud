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

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Authorize(Policy = "projectRead")]
    [Route("api/projects/{projectId:guid}/users")]
    public class ProjectUsersController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly IProjectsRepositoryReadOnly projectsRepository;

        public ProjectUsersController(UserService userService, Orchestrator orchestrator, IProjectsRepositoryReadOnly projectsRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        public Guid? ProjectId
        {
            get
            {
                var projectId = RouteData.Values.GetValueOrDefault(nameof(ProjectId), StringComparison.OrdinalIgnoreCase)?.ToString();

                return (string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId));
            }
        }

        private User CurrentUser => new User()
        {
            Id = userService.CurrentUserId,
            Role = UserRoles.Project.Owner
        };

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!ProjectId.HasValue)
                return new NotFoundResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return new NotFoundResult();

            var users = project?.Users;

            if (users is null)
                return new NotFoundResult();

            return new OkObjectResult(users);
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> Get(Guid userId)
        {
            if (!ProjectId.HasValue) return new NotFoundResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return new NotFoundResult();

            var user = project?.Users?.FirstOrDefault(u => u.Id == userId);

            if (user is null)
                return new NotFoundResult();

            return new OkObjectResult(user); ;
        }

        [HttpPost]
        [Authorize(Policy = "projectCreate")]
        public async Task<IActionResult> Post([FromBody] UserDefinition userDefinition)
        {
            if (!ProjectId.HasValue)
                return new NotFoundResult();

            if (userDefinition is null)
                return new BadRequestResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return new NotFoundResult();

            var newUser = await userService
                .GetUserAsync(userDefinition)
                .ConfigureAwait(false);

            if (newUser is null)
                return new NotFoundResult();

            if (project.Users.Contains(newUser))
                return new ConflictObjectResult("User already esists in this Project.");

            var command = new ProjectUserCreateCommand(CurrentUser, newUser, ProjectId.Value);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            return commandResult.ActionResult();
        }

        [HttpPut]
        [Authorize(Policy = "projectCreate")]
        public async Task<IActionResult> Put([FromBody] User user)
        {
            if (!ProjectId.HasValue)
                return new NotFoundResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return new NotFoundResult();

            var oldUser = project?.Users?.FirstOrDefault(u => u.Id == user.Id);

            if (oldUser is null)
                return new NotFoundObjectResult("User does not esists in this Project.");

            var command = new ProjectUserUpdateCommand(CurrentUser, user, ProjectId.Value);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            return commandResult.ActionResult();
        }

        [HttpDelete("{userId:guid}")]
        [Authorize(Policy = "projectCreate")]
        public async Task<IActionResult> Delete(Guid userId)
        {
            if (!ProjectId.HasValue)
                return new NotFoundResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return new NotFoundResult();

            var user = project?.Users?.FirstOrDefault(u => u.Id == userId);

            if (user is null)
                return new NotFoundObjectResult("User does not esists in this Project.");

            var command = new ProjectUserDeleteCommand(CurrentUser, user, ProjectId.Value);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            return commandResult.ActionResult();
        }
    }
}

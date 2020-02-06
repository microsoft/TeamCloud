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
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCodes.ValidationError)
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{ProjectId.Value}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            var users = project?.Users ?? new List<User>();

            return DataResult<List<User>>
                .Ok(users)
                .ActionResult();
        }


        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> Get(Guid userId)
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCodes.ValidationError)
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{ProjectId.Value}' could not be found in this TeamCloud Instance.")
                    .ActionResult();

            var user = project?.Users?.FirstOrDefault(u => u.Id == userId);

            if (user is null)
                return ErrorResult
                    .NotFound($"A User with the ID '{userId}' could not be found in this Project.")
                    .ActionResult();

            return DataResult<User>
                .Ok(user)
                .ActionResult();
        }


        [HttpPost]
        [Authorize(Policy = "projectCreate")]
        public async Task<IActionResult> Post([FromBody] UserDefinition userDefinition)
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCodes.ValidationError)
                    .ActionResult();

            var validation = new UserDefinitionValidator().Validate(userDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{ProjectId.Value}' could not be found in this TeamCloud Instance.")
                    .ActionResult();

            var newUser = await userService
                .GetUserAsync(userDefinition)
                .ConfigureAwait(false);

            if (newUser is null)
                return ErrorResult
                    .NotFound($"A User with the Email '{userDefinition.Email}' could not be found.")
                    .ActionResult();

            if (project.Users.Contains(newUser))
                return ErrorResult
                    .Conflict($"A User with the Email '{userDefinition.Email}' already exists on this Project. Please try your request again with a unique email or call PUT to update the existing User.")
                    .ActionResult();

            var command = new ProjectUserCreateCommand(CurrentUser, newUser, ProjectId.Value);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shoudn't happen, but we need to decide to do when it does...");
        }

        [HttpPut]
        [Authorize(Policy = "projectCreate")]
        public async Task<IActionResult> Put([FromBody] User user)
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCodes.ValidationError)
                    .ActionResult();

            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{ProjectId.Value}' could not be found in this TeamCloud Instance.")
                    .ActionResult();

            var oldUser = project?.Users?.FirstOrDefault(u => u.Id == user.Id);

            if (oldUser is null)
                return ErrorResult
                    .NotFound($"A User with the ID '{oldUser.Id}' could not be found on this Project.")
                    .ActionResult();

            var command = new ProjectUserUpdateCommand(CurrentUser, user, ProjectId.Value);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shoudn't happen, but we need to decide to do when it does...");
        }

        [HttpDelete("{userId:guid}")]
        [Authorize(Policy = "projectCreate")]
        public async Task<IActionResult> Delete(Guid userId)
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCodes.ValidationError)
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{ProjectId.Value}' could not be found in this TeamCloud Instance.")
                    .ActionResult();

            var user = project?.Users?.FirstOrDefault(u => u.Id == userId);

            if (user is null)
                return ErrorResult
                    .NotFound($"A User with the ID '{userId}' could not be found on this Project.")
                    .ActionResult();

            var command = new ProjectUserDeleteCommand(CurrentUser, user, ProjectId.Value);

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

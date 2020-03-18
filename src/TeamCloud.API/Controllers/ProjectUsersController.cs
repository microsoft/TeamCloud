/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Data;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/users")]
    [Produces("application/json")]
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
        [Authorize(Policy = "projectRead")]
        [SwaggerOperation(OperationId = "GetProjectUsers", Summary = "Gets all Users for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Users", typeof(DataResult<List<User>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The projectId provided in the path was invalid.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
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
                .Ok(users.ToList())
                .ActionResult();
        }


        [HttpGet("{userNameOrId:userNameOrId}")]
        [Authorize(Policy = "projectRead")]
        [SwaggerOperation(OperationId = "GetProjectUserByNameOrId", Summary = "Gets a Project User by ID or email address.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project User", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The projectId provided in the path was invalid.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the provided identifier was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get([FromRoute] string userNameOrId)
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            if (string.IsNullOrWhiteSpace(userNameOrId))
                return ErrorResult
                    .BadRequest($"The identifier '{userNameOrId}' provided in the url path is invalid.  Must be a valid email address or GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{ProjectId.Value}' could not be found in this TeamCloud Instance.")
                    .ActionResult();

            if (!Guid.TryParse(userNameOrId, out var userId))
            {
                var idLookup = await userService
                    .GetUserIdAsync(userNameOrId)
                    .ConfigureAwait(false);

                if (!idLookup.HasValue || idLookup.Value == Guid.Empty)
                    return ErrorResult
                        .NotFound($"A User with the email '{userNameOrId}' could not be found.")
                        .ActionResult();

                userId = idLookup.Value;
            }

            var user = project?.Users?.FirstOrDefault(u => u.Id == userId);

            if (user is null)
                return ErrorResult
                    .NotFound($"The specified User could not be found in this Project.")
                    .ActionResult();

            return DataResult<User>
                .Ok(user)
                .ActionResult();
        }

        [HttpPost]
        [Authorize(Policy = "projectCreate")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectUser", Summary = "Creates a new Project User")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Project User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The projectId provided in the path was invalid.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the email address provided in the request body was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] UserDefinition userDefinition)
        {
            if (userDefinition is null)
                throw new ArgumentNullException(nameof(userDefinition));

            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
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

            var command = new OrchestratorProjectUserCreateCommand(CurrentUser, newUser, ProjectId.Value);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shoudn't happen, but we need to decide to do when it does.");
        }



        [HttpPut]
        [Authorize(Policy = "projectCreate")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectUser", Summary = "Updates an existing Project User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project UserProject. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The projectId provided in the path was invalid, or the User provided in the request body did not pass validation.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromBody] User user)
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
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

            var command = new OrchestratorProjectUserUpdateCommand(CurrentUser, user, ProjectId.Value);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shoudn't happen, but we need to decide to do when it does.");
        }



        [HttpDelete("{userNameOrId:userNameOrId}")]
        [Authorize(Policy = "projectCreate")]
        [SwaggerOperation(OperationId = "DeleteProjectUser", Summary = "Deletes an existing Project User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The projectId provided in the path was invalid.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the provided identifier was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete([FromRoute]string userNameOrId)
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            if (string.IsNullOrWhiteSpace(userNameOrId))
                return ErrorResult
                    .BadRequest($"The identifier '{userNameOrId}' provided in the url path is invalid.  Must be a valid email address or GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{ProjectId.Value}' could not be found in this TeamCloud Instance.")
                    .ActionResult();

            if (!Guid.TryParse(userNameOrId, out var userId))
            {
                var idLookup = await userService
                    .GetUserIdAsync(userNameOrId)
                    .ConfigureAwait(false);

                if (!idLookup.HasValue || idLookup.Value == Guid.Empty)
                    return ErrorResult
                        .NotFound($"A User with the email '{userNameOrId}' could not be found.")
                        .ActionResult();

                userId = idLookup.Value;
            }

            var user = project?.Users?.FirstOrDefault(u => u.Id == userId);

            if (user is null)
                return ErrorResult
                    .NotFound($"The specified User could not be found in this Project.")
                    .ActionResult();

            var command = new OrchestratorProjectUserDeleteCommand(CurrentUser, user, ProjectId.Value);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shoudn't happen, but we need to decide to do when it does.");
        }
    }
}

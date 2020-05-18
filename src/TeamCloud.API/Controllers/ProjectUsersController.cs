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
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/users")]
    [Produces("application/json")]
    public class ProjectUsersController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly IUsersRepositoryReadOnly usersRepository;

        public ProjectUsersController(UserService userService, Orchestrator orchestrator, IUsersRepositoryReadOnly usersRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        public Guid? ProjectId
        {
            get
            {
                var projectId = RouteData.Values.GetValueOrDefault(nameof(ProjectId), StringComparison.OrdinalIgnoreCase)?.ToString();

                return string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId);
            }
        }


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

            var projectId = ProjectId.Value;

            var users = await usersRepository
                .ListAsync(projectId)
                .ToListAsync()
                .ConfigureAwait(false);

            var projectUsers = (users ?? new List<User>())
                .Select(u => new ProjectUser(u, projectId));

            return DataResult<List<ProjectUser>>
                .Ok(projectUsers.ToList())
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

            var projectId = ProjectId.Value;

            if (string.IsNullOrWhiteSpace(userNameOrId))
                return ErrorResult
                    .BadRequest($"The identifier '{userNameOrId}' provided in the url path is invalid.  Must be a valid email address or GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var userId = await userService
                .GetUserIdAsync(userNameOrId)
                .ConfigureAwait(false);

            if (!userId.HasValue || userId.Value == Guid.Empty)
                return ErrorResult
                    .NotFound($"The user '{userNameOrId}' could not be found.")
                    .ActionResult();

            var user = await usersRepository
                .GetAsync(userId.Value)
                .ConfigureAwait(false);

            if (user is null || !user.IsMember(projectId))
                return ErrorResult
                    .NotFound($"The specified User could not be found in this Project.")
                    .ActionResult();

            var projectUser = new ProjectUser(user, projectId);

            return DataResult<ProjectUser>
                .Ok(projectUser)
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

            var projectId = ProjectId.Value;

            var validation = new ProjectUserDefinitionValidator().Validate(userDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var user = await userService
                .ResolveUserAsync(userDefinition)
                .ConfigureAwait(false);

            if (user is null)
                return ErrorResult
                    .NotFound($"The user '{userDefinition.Identifier}' could not be found.")
                    .ActionResult();

            if (user.IsMember(projectId))
                return ErrorResult
                    .Conflict($"The user '{userDefinition.Identifier}' already exists on this Project. Please try your request again with a unique user or call PUT to update the existing User.")
                    .ActionResult();

            user.EnsureProjectMembership(projectId, Enum.Parse<ProjectUserRole>(userDefinition.Role, true), userDefinition.Properties);

            var currentUserForCommand = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProjectUserCreateCommand(currentUserForCommand, user, projectId);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shouldn't happen, but we need to decide to do when it does.");
        }


        [HttpPut]
        [Authorize(Policy = "projectCreate")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectUser", Summary = "Updates an existing Project User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project UserProject. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The projectId provided in the path was invalid, or the User provided in the request body did not pass validation.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromBody] ProjectUser user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var projectId = ProjectId.Value;

            var validation = new ProjectUserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var oldUser = await usersRepository
                .GetAsync(user.Id)
                .ConfigureAwait(false);

            if (oldUser is null || !oldUser.IsMember(projectId))
                return ErrorResult
                    .NotFound($"The user '{user.Id}' could not be found in this project.")
                    .ActionResult();

            if (oldUser.IsOwner(projectId) && !user.IsOwner())
            {
                var otherOwners = await usersRepository
                    .ListOwnersAsync(projectId)
                    .AnyAsync(o => o.Id != user.Id)
                    .ConfigureAwait(false);

                if (!otherOwners)
                    return ErrorResult
                        .BadRequest($"Projects must have at least one Owner. To change this user's role you must first add another Owner.", ResultErrorCode.ValidationError)
                        .ActionResult();
            }

            var membership = new ProjectMembership
            {
                ProjectId = projectId,
                Role = user.Role,
                Properties = user.Properties
            };

            if (!oldUser.HasEqualMembership(membership))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships did not change." })
                    .ActionResult();

            oldUser.EnsureProjectMembership(membership);

            var currentUserForCommand = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProjectUserUpdateCommand(currentUserForCommand, oldUser, projectId);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shouldn't happen, but we need to decide to do when it does.");
        }


        [HttpDelete("{userNameOrId:userNameOrId}")]
        [Authorize(Policy = "projectCreate")]
        [SwaggerOperation(OperationId = "DeleteProjectUser", Summary = "Deletes an existing Project User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The projectId provided in the path was invalid.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the provided identifier was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete([FromRoute] string userNameOrId)
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var projectId = ProjectId.Value;

            if (string.IsNullOrWhiteSpace(userNameOrId))
                return ErrorResult
                    .BadRequest($"The identifier '{userNameOrId}' provided in the url path is invalid.  Must be a valid email address or GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var userId = await userService
                .GetUserIdAsync(userNameOrId)
                .ConfigureAwait(false);

            if (!userId.HasValue || userId.Value == Guid.Empty)
                return ErrorResult
                    .NotFound($"The user '{userNameOrId}' could not be found.")
                    .ActionResult();

            var user = await usersRepository
                .GetAsync(userId.Value)
                .ConfigureAwait(false);

            if (user is null || !user.IsMember(projectId))
                return ErrorResult
                    .NotFound($"The specified User could not be found in this Project.")
                    .ActionResult();

            if (user.IsOwner(projectId))
            {
                var otherOwners = await usersRepository
                    .ListOwnersAsync(projectId)
                    .AnyAsync(o => o.Id != userId)
                    .ConfigureAwait(false);

                if (!otherOwners)
                    return ErrorResult
                        .BadRequest($"Projects must have at least one Owner. To delete this user you must first add another Owner.", ResultErrorCode.ValidationError)
                        .ActionResult();
            }

            var currentUserForCommand = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProjectUserDeleteCommand(currentUserForCommand, user, projectId);

            var commandResult = await orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            if (commandResult.Links.TryGetValue("status", out var statusUrl))
                return StatusResult
                    .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
                    .ActionResult();

            throw new Exception("This shouldn't happen, but we need to decide to do when it does.");
        }
    }
}

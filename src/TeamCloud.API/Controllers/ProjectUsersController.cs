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
using TeamCloud.API.Auth;
using TeamCloud.API.Data;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;
using TeamCloud.Model.Internal.Commands;
using TeamCloud.Model.Internal.Data;
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
        readonly IUsersRepository usersRepository;

        public ProjectUsersController(UserService userService, Orchestrator orchestrator, IUsersRepository usersRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }

        public string ProjectId
            => RouteData.Values.GetValueOrDefault(nameof(ProjectId), StringComparison.OrdinalIgnoreCase)?.ToString();


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectUsers", Summary = "Gets all Users for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Users", typeof(DataResult<List<User>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            if (string.IsNullOrEmpty(ProjectId))
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var users = await usersRepository
                .ListAsync(ProjectId)
                .ToListAsync()
                .ConfigureAwait(false);

            var returnUsers = users.Select(u => u.PopulateExternalModel()).ToList();

            return DataResult<List<User>>
                .Ok(returnUsers)
                .ActionResult();
        }


        [HttpGet("{userNameOrId:userNameOrId}")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectUserByNameOrId", Summary = "Gets a Project User by ID or email address.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project User", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the provided identifier was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get([FromRoute] string userNameOrId)
        {
            if (string.IsNullOrEmpty(ProjectId))
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            if (string.IsNullOrWhiteSpace(userNameOrId))
                return ErrorResult
                    .BadRequest($"The identifier '{userNameOrId}' provided in the url path is invalid.  Must be a valid email address or GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var userId = await userService
                .GetUserIdAsync(userNameOrId)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(userId))
                return ErrorResult
                    .NotFound($"The user '{userNameOrId}' could not be found.")
                    .ActionResult();

            var user = await usersRepository
                .GetAsync(userId)
                .ConfigureAwait(false);

            if (user is null || !user.IsMember(ProjectId))
                return ErrorResult
                    .NotFound($"The specified User could not be found in this Project.")
                    .ActionResult();

            var returnUser = user.PopulateExternalModel(ProjectId);

            return DataResult<User>
                .Ok(returnUser)
                .ActionResult();
        }


        [HttpGet("me")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectUserMe", Summary = "Gets a Project User for the calling user.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project User", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User matching the current user was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> GetMe()
        {
            if (string.IsNullOrEmpty(ProjectId))
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var me = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            if (me is null)
                return ErrorResult
                    .NotFound($"A User matching the current user was not found in this TeamCloud instance.")
                    .ActionResult();

            if (!me.IsMember(ProjectId))
                return ErrorResult
                    .NotFound($"A User matching the current user was not found in this Project.")
                    .ActionResult();

            var returnUser = me.PopulateExternalModel(ProjectId);

            return DataResult<User>
                .Ok(returnUser)
                .ActionResult();
        }

        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectUserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectUser", Summary = "Creates a new Project User")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Project User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the email address provided in the request body was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] UserDefinition userDefinition)
        {
            if (userDefinition is null)
                throw new ArgumentNullException(nameof(userDefinition));

            if (string.IsNullOrEmpty(ProjectId))
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var validation = new UserDefinitionProjectValidator().Validate(userDefinition);

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

            if (user.IsMember(ProjectId))
                return ErrorResult
                    .Conflict($"The user '{userDefinition.Identifier}' already exists on this Project. Please try your request again with a unique user or call PUT to update the existing User.")
                    .ActionResult();

            user.EnsureProjectMembership(ProjectId, Enum.Parse<ProjectUserRole>(userDefinition.Role, true), userDefinition.Properties);

            var currentUserForCommand = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProjectUserCreateCommand(currentUserForCommand, user, ProjectId);

            return await orchestrator
                .InvokeAndReturnAccepted(command)
                .ConfigureAwait(false);
        }


        [HttpPut("{userNameOrId:userNameOrId}")]
        [Authorize(Policy = AuthPolicies.ProjectUserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectUser", Summary = "Updates an existing Project User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project UserProject. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromRoute] string userNameOrId, [FromBody] User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(ProjectId))
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            if (string.IsNullOrWhiteSpace(userNameOrId))
                return ErrorResult
                    .BadRequest($"The identifier '{userNameOrId}' provided in the url path is invalid.  Must be a valid email address or GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var userId = await userService
                .GetUserIdAsync(userNameOrId)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(userId))
                return ErrorResult
                    .NotFound($"The user '{userNameOrId}' could not be found.")
                    .ActionResult();

            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            if (!userId.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"User's id does match the identifier provided in the path." })
                    .ActionResult();

            var oldUser = await usersRepository
                .GetAsync(user.Id)
                .ConfigureAwait(false);

            if (oldUser is null || !oldUser.IsMember(ProjectId))
                return ErrorResult
                    .NotFound($"The user '{user.Id}' could not be found in this project.")
                    .ActionResult();

            if (oldUser.IsOwner(ProjectId) && !user.IsOwner(ProjectId))
            {
                var otherOwners = await usersRepository
                    .ListOwnersAsync(ProjectId)
                    .AnyAsync(o => o.Id.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                    .ConfigureAwait(false);

                if (!otherOwners)
                    return ErrorResult
                        .BadRequest($"Projects must have at least one Owner. To change this user's role you must first add another Owner.", ResultErrorCode.ValidationError)
                        .ActionResult();
            }

            var membership = user.ProjectMembership(ProjectId);

            if (oldUser.HasEqualMembership(membership))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships did not change." })
                    .ActionResult();

            oldUser.UpdateProjectMembership(membership);

            var currentUserForCommand = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProjectUserUpdateCommand(currentUserForCommand, oldUser, ProjectId);

            return await orchestrator
                .InvokeAndReturnAccepted(command)
                .ConfigureAwait(false);
        }


        [HttpPut("me")]
        [Authorize(Policy = AuthPolicies.ProjectUserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectUserMe", Summary = "Updates an existing Project User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User matching the current user was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> PutMe([FromBody] User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(ProjectId))
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var me = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            if (me is null)
                return ErrorResult
                    .NotFound($"A User matching the current user was not found in this TeamCloud instance.")
                    .ActionResult();

            if (!me.Id.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"User's id does match the id of the current user." })
                    .ActionResult();

            if (!me.IsMember(ProjectId))
                return ErrorResult
                    .NotFound($"A User matching the current user was not found in this Project.")
                    .ActionResult();

            if (me.IsOwner(ProjectId) && !user.IsOwner(ProjectId))
            {
                var otherOwners = await usersRepository
                    .ListOwnersAsync(ProjectId)
                    .AnyAsync(o => o.Id.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                    .ConfigureAwait(false);

                if (!otherOwners)
                    return ErrorResult
                        .BadRequest($"Projects must have at least one Owner. To change this user's role you must first add another Owner.", ResultErrorCode.ValidationError)
                        .ActionResult();
            }

            var membership = user.ProjectMembership(ProjectId);

            if (me.HasEqualMembership(membership))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships did not change." })
                    .ActionResult();

            me.UpdateProjectMembership(membership);

            var currentUserForCommand = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProjectUserUpdateCommand(currentUserForCommand, me, ProjectId);

            return await orchestrator
                .InvokeAndReturnAccepted(command)
                .ConfigureAwait(false);
        }


        [HttpDelete("{userNameOrId:userNameOrId}")]
        [Authorize(Policy = AuthPolicies.ProjectUserWrite)]
        [SwaggerOperation(OperationId = "DeleteProjectUser", Summary = "Deletes an existing Project User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the provided identifier was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete([FromRoute] string userNameOrId)
        {
            if (string.IsNullOrEmpty(ProjectId))
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            if (string.IsNullOrWhiteSpace(userNameOrId))
                return ErrorResult
                    .BadRequest($"The identifier '{userNameOrId}' provided in the url path is invalid.  Must be a valid email address or GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var userId = await userService
                .GetUserIdAsync(userNameOrId)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(userId))
                return ErrorResult
                    .NotFound($"The user '{userNameOrId}' could not be found.")
                    .ActionResult();

            var user = await usersRepository
                .GetAsync(userId)
                .ConfigureAwait(false);

            if (user is null || !user.IsMember(ProjectId))
                return ErrorResult
                    .NotFound($"The specified User could not be found in this Project.")
                    .ActionResult();

            if (user.IsOwner(ProjectId))
            {
                var otherOwners = await usersRepository
                    .ListOwnersAsync(ProjectId)
                    .AnyAsync(o => o.Id.Equals(userId, StringComparison.OrdinalIgnoreCase))
                    .ConfigureAwait(false);

                if (!otherOwners)
                    return ErrorResult
                        .BadRequest($"Projects must have at least one Owner. To delete this user you must first add another Owner.", ResultErrorCode.ValidationError)
                        .ActionResult();
            }

            var currentUserForCommand = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProjectUserDeleteCommand(currentUserForCommand, user, ProjectId);

            return await orchestrator
                .InvokeAndReturnAccepted(command)
                .ConfigureAwait(false);
        }
    }
}

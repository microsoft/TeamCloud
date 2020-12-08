/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Data;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Data.Validators;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{org}/projects/{projectId:projectId}/users")]
    [Produces("application/json")]
    public class ProjectUsersController : ApiController
    {
        private readonly IUserRepository userRepository;

        public ProjectUsersController(IUserRepository userRepository) : base()
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetProjectUsers", Summary = "Gets all Users for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Users", typeof(DataResult<List<User>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => EnsureProjectIdAsync(async projectId =>
        {
            var users = await userRepository
                .ListAsync(OrgId, projectId)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<User>>
                .Ok(users)
                .ToActionResult();
        });


        [HttpGet("{userId:userId}")]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetProjectUser", Summary = "Gets a Project User by ID or email address.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project User", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the provided identifier was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Get([FromRoute] string userId) => EnsureProjectAndUserAsync((projectId, user) =>
        {
            if (!user.IsMember(projectId))
                return ErrorResult
                    .NotFound($"The specified User could not be found in this Project.")
                    .ToActionResult();

            return DataResult<User>
                .Ok(user)
                .ToActionResult();
        });


        [HttpGet("me")]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetProjectUserMe", Summary = "Gets a Project User for the calling user.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project User", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User matching the current user was not found.", typeof(ErrorResult))]
        public Task<IActionResult> GetMe() => EnsureProjectAndCurrentUserAsync((projectId, user) =>
        {
            if (!user.IsMember(projectId))
                return ErrorResult
                    .NotFound($"A User matching the current user was not found in this Project.")
                    .ToActionResult();

            return DataResult<User>
                .Ok(user)
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectUserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectUser", Summary = "Creates a new Project User")]
        [SwaggerResponse(StatusCodes.Status201Created, "The new Project User was created.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Project User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the email address provided in the request body was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] UserDefinition userDefinition) => EnsureProjectAndCurrentUserAsync(async (projectId, currentUser) =>
        {
            if (userDefinition is null)
                throw new ArgumentNullException(nameof(userDefinition));

            var validation = new ProjectUserDefinitionValidator().Validate(userDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            var user = await UserService
                .ResolveUserAsync(OrgId, userDefinition)
                .ConfigureAwait(false);

            if (user is null)
                return ErrorResult
                    .NotFound($"The user '{userDefinition.Identifier}' could not be found.")
                    .ToActionResult();

            if (user.IsMember(projectId))
                return ErrorResult
                    .Conflict($"The user '{userDefinition.Identifier}' already exists on this Project. Please try your request again with a unique user or call PUT to update the existing User.")
                    .ToActionResult();

            user.EnsureProjectMembership(projectId, Enum.Parse<ProjectUserRole>(userDefinition.Role, true), userDefinition.Properties);

            var command = new ProjectUserCreateCommand(currentUser, user, projectId);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPut("{userId:userId}")]
        [Authorize(Policy = AuthPolicies.ProjectUserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectUser", Summary = "Updates an existing Project User.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The Project User was updated.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project UserProject. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Put([FromRoute] string userId, [FromBody] User user) => EnsureProjectAndUserAsync(async (projectId, existingUser) =>
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            if (!user.Id.Equals(existingUser.Id, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"User's id does match the identifier provided in the path." })
                    .ToActionResult();

            if (!existingUser.IsMember(projectId))
                return ErrorResult
                    .NotFound($"The user '{user.Id}' could not be found in this project.")
                    .ToActionResult();

            if (existingUser.IsOwner(projectId) && !user.IsOwner(projectId))
                return ErrorResult
                    .BadRequest($"Projects must have an owner. To change this user's role you must first transfer ownersip to another user.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var membership = user.ProjectMembership(projectId);

            if (existingUser.HasEqualMembership(membership))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships did not change." })
                    .ToActionResult();

            existingUser.UpdateProjectMembership(membership);

            var currentUser = await UserService
                .CurrentUserAsync(OrgId)
                .ConfigureAwait(false);

            var command = new ProjectUserUpdateCommand(currentUser, existingUser, projectId);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPut("me")]
        [Authorize(Policy = AuthPolicies.ProjectUserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectUserMe", Summary = "Updates an existing Project User.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The Project User was updated.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User matching the current user was not found.", typeof(ErrorResult))]
        public Task<IActionResult> PutMe([FromBody] User user) => EnsureProjectAndCurrentUserAsync(async (projectId, currentUser) =>
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            if (!user.Id.Equals(currentUser.Id, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"User's id does match the id of the current user." })
                    .ToActionResult();

            if (!currentUser.IsMember(projectId))
                return ErrorResult
                    .NotFound($"A User matching the current user was not found in this Project.")
                    .ToActionResult();

            if (currentUser.IsOwner(projectId) && !user.IsOwner(projectId))
                return ErrorResult
                    .BadRequest($"Projects must have an owner. To change this user's role you must first transfer ownersip to another user.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var membership = user.ProjectMembership(projectId);

            if (currentUser.HasEqualMembership(membership))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships did not change." })
                    .ToActionResult();

            currentUser.UpdateProjectMembership(membership);

            var command = new ProjectUserUpdateCommand(currentUser, currentUser, projectId);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{userId:userId}")]
        [Authorize(Policy = AuthPolicies.ProjectUserWrite)]
        [SwaggerOperation(OperationId = "DeleteProjectUser", Summary = "Deletes an existing Project User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the provided identifier was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Delete([FromRoute] string userId) => EnsureProjectAndUserAsync(async (projectId, user) =>
        {
            if (!user.IsMember(projectId))
                return ErrorResult
                    .NotFound($"The specified User could not be found in this Project.")
                    .ToActionResult();

            if (user.IsOwner(projectId))
                return ErrorResult
                    .BadRequest($"Projects must have an owner. To change this user's role you must first transfer ownersip to another user.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync(OrgId)
                .ConfigureAwait(false);

            var command = new ProjectUserDeleteCommand(currentUser, user, projectId);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });
    }
}

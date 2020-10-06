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
    [Route("api/projects/{projectId:guid}/users")]
    [Produces("application/json")]
    public class ProjectUsersController : ApiController
    {
        public ProjectUsersController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository, IUserRepository userRepository)
            : base(userService, orchestrator, projectRepository, userRepository)
        { }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectUsers", Summary = "Gets all Users for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Users", typeof(DataResult<List<User>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => EnsureProjectAsync(async project =>
        {
            var userDocuments = await UserRepository
                .ListAsync(project.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            var users = userDocuments.Select(u => u.PopulateExternalModel()).ToList();

            return DataResult<List<User>>
                .Ok(users)
                .ToActionResult();
        });


        [HttpGet("{userNameOrId:userNameOrId}")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectUserByNameOrId", Summary = "Gets a Project User by ID or email address.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project User", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the provided identifier was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Get([FromRoute] string userNameOrId) => EnsureProjectAndUserAsync((project, user) =>
        {
            if (!user.IsMember(project.Id))
                return ErrorResult
                    .NotFound($"The specified User could not be found in this Project.")
                    .ToActionResult();

            return DataResult<User>
                .Ok(user.PopulateExternalModel(project.Id))
                .ToActionResult();
        });


        [HttpGet("me")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectUserMe", Summary = "Gets a Project User for the calling user.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project User", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User matching the current user was not found.", typeof(ErrorResult))]
        public Task<IActionResult> GetMe() => EnsureProjectAndCurrentUserAsync((project, user) =>
        {
            if (!user.IsMember(project.Id))
                return ErrorResult
                    .NotFound($"A User matching the current user was not found in this Project.")
                    .ToActionResult();

            return DataResult<User>
                .Ok(user.PopulateExternalModel(project.Id))
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectUserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectUser", Summary = "Creates a new Project User")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Project User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the email address provided in the request body was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] UserDefinition userDefinition) => EnsureProjectAndCurrentUserAsync(async (project, currentUser) =>
        {
            if (userDefinition is null)
                throw new ArgumentNullException(nameof(userDefinition));

            var validation = new UserDefinitionProjectValidator().Validate(userDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            var userDocument = await UserService
                .ResolveUserAsync(userDefinition)
                .ConfigureAwait(false);

            if (userDocument is null)
                return ErrorResult
                    .NotFound($"The user '{userDefinition.Identifier}' could not be found.")
                    .ToActionResult();

            if (userDocument.IsMember(project.Id))
                return ErrorResult
                    .Conflict($"The user '{userDefinition.Identifier}' already exists on this Project. Please try your request again with a unique user or call PUT to update the existing User.")
                    .ToActionResult();

            userDocument.EnsureProjectMembership(project.Id, Enum.Parse<ProjectUserRole>(userDefinition.Role, true), userDefinition.Properties);

            var command = new OrchestratorProjectUserCreateCommand(currentUser, userDocument, project.Id);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<UserDocument, User>(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPut("{userNameOrId:userNameOrId}")]
        [Authorize(Policy = AuthPolicies.ProjectUserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectUser", Summary = "Updates an existing Project User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project UserProject. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Put([FromRoute] string userNameOrId, [FromBody] User user) => EnsureProjectAndUserAsync(async (project, userDocument) =>
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            if (!user.Id.Equals(userDocument.Id, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"User's id does match the identifier provided in the path." })
                    .ToActionResult();

            if (!userDocument.IsMember(project.Id))
                return ErrorResult
                    .NotFound($"The user '{user.Id}' could not be found in this project.")
                    .ToActionResult();

            if (userDocument.IsOwner(project.Id) && !user.IsOwner(project.Id))
            {
                var otherOwners = await UserRepository
                    .ListOwnersAsync(project.Id)
                    .AnyAsync(o => o.Id.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                    .ConfigureAwait(false);

                if (!otherOwners)
                    return ErrorResult
                        .BadRequest($"Projects must have at least one Owner. To change this user's role you must first add another Owner.", ResultErrorCode.ValidationError)
                        .ToActionResult();
            }

            var membership = user.ProjectMembership(project.Id);

            if (userDocument.HasEqualMembership(membership))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships did not change." })
                    .ToActionResult();

            userDocument.UpdateProjectMembership(membership);

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProjectUserUpdateCommand(currentUser, userDocument, project.Id);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<UserDocument, User>(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPut("me")]
        [Authorize(Policy = AuthPolicies.ProjectUserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectUserMe", Summary = "Updates an existing Project User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User matching the current user was not found.", typeof(ErrorResult))]
        public Task<IActionResult> PutMe([FromBody] User user) => EnsureProjectAndCurrentUserAsync(async (project, currentUser) =>
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

            if (!currentUser.IsMember(project.Id))
                return ErrorResult
                    .NotFound($"A User matching the current user was not found in this Project.")
                    .ToActionResult();

            if (currentUser.IsOwner(project.Id) && !user.IsOwner(project.Id))
            {
                var otherOwners = await UserRepository
                    .ListOwnersAsync(project.Id)
                    .AnyAsync(o => o.Id.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                    .ConfigureAwait(false);

                if (!otherOwners)
                    return ErrorResult
                        .BadRequest($"Projects must have at least one Owner. To change this user's role you must first add another Owner.", ResultErrorCode.ValidationError)
                        .ToActionResult();
            }

            var membership = user.ProjectMembership(project.Id);

            if (currentUser.HasEqualMembership(membership))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships did not change." })
                    .ToActionResult();

            currentUser.UpdateProjectMembership(membership);

            var command = new OrchestratorProjectUserUpdateCommand(currentUser, currentUser, project.Id);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<UserDocument, User>(command, Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{userNameOrId:userNameOrId}")]
        [Authorize(Policy = AuthPolicies.ProjectUserWrite)]
        [SwaggerOperation(OperationId = "DeleteProjectUser", Summary = "Deletes an existing Project User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the provided identifier was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Delete([FromRoute] string userNameOrId) => EnsureProjectAndUserAsync(async (project, user) =>
        {
            if (!user.IsMember(project.Id))
                return ErrorResult
                    .NotFound($"The specified User could not be found in this Project.")
                    .ToActionResult();

            if (user.IsOwner(project.Id))
            {
                var otherOwners = await UserRepository
                    .ListOwnersAsync(project.Id)
                    .AnyAsync(o => o.Id.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                    .ConfigureAwait(false);

                if (!otherOwners)
                    return ErrorResult
                        .BadRequest($"Projects must have at least one Owner. To delete this user you must first add another Owner.", ResultErrorCode.ValidationError)
                        .ToActionResult();
            }

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProjectUserDeleteCommand(currentUser, user, project.Id);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<UserDocument, User>(command, Request)
                .ConfigureAwait(false);
        });
    }
}

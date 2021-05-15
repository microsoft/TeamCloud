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
using TeamCloud.API.Controllers.Core;
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
    [Route("orgs/{organizationId:organizationId}/projects/{projectId:projectId}/users")]
    [Produces("application/json")]
    public class ProjectUsersController : TeamCloudController
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
        public Task<IActionResult> Get() => ExecuteAsync<TeamCloudProjectContext>(async context =>
        {
            var users = await userRepository
                .ListAsync(context.Organization.Id, context.Project.Id)
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
        public Task<IActionResult> Get([FromRoute] string userId) => ExecuteAsync<TeamCloudProjectUserContext>(context =>
        {
            return DataResult<User>
                .Ok(context.User)
                .ToActionResultAsync();
        });


        [HttpGet("me")]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetProjectUserMe", Summary = "Gets a Project User for the calling user.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project User", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User matching the current user was not found.", typeof(ErrorResult))]
        public Task<IActionResult> GetMe() => ExecuteAsync<TeamCloudProjectContext>(context =>
        {
            if (!context.ContextUser.IsMember(context.Project.Id))
                return ErrorResult
                    .NotFound($"A User matching the current user was not found in this Project.")
                    .ToActionResultAsync();

            return DataResult<User>
                .Ok(context.ContextUser)
                .ToActionResultAsync();
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
        public Task<IActionResult> Post([FromBody] UserDefinition userDefinition) => ExecuteAsync<TeamCloudProjectContext>(async context =>
        {
            if (userDefinition is null)
                throw new ArgumentNullException(nameof(userDefinition));

            var validation = new ProjectUserDefinitionValidator().Validate(userDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            var user = await UserService
                .ResolveUserAsync(context.Organization.Id, userDefinition)
                .ConfigureAwait(false);

            if (user is null)
                return ErrorResult
                    .NotFound($"The user '{userDefinition.Identifier}' could not be found.")
                    .ToActionResult();

            if (user.IsMember(context.Project.Id))
                return ErrorResult
                    .Conflict($"The user '{userDefinition.Identifier}' already exists on this Project. Please try your request again with a unique user or call PUT to update the existing User.")
                    .ToActionResult();

            if (user.Role == OrganizationUserRole.None)
            {
                // a user added to a project that doesn't exist
                // on the org level must become a member of the org
                // and not must not assigned to the none role

                user.Role = OrganizationUserRole.Member;
            }

            user.EnsureProjectMembership(context.Project.Id, Enum.Parse<ProjectUserRole>(userDefinition.Role, true), userDefinition.Properties);

            var command = new ProjectUserCreateCommand(context.ContextUser, user, context.Project.Id);

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
        public Task<IActionResult> Put([FromRoute] string userId, [FromBody] User user) => ExecuteAsync<TeamCloudProjectUserContext>(async context =>
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            if (!user.Id.Equals(context.User.Id, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"User's id does match the identifier provided in the path." })
                    .ToActionResult();

            if (!context.User.IsMember(context.Project.Id))
                return ErrorResult
                    .NotFound($"The user '{user.Id}' could not be found in this project.")
                    .ToActionResult();

            if (context.User.IsOwner(context.Project.Id) && !user.IsOwner(context.Project.Id))
                return ErrorResult
                    .BadRequest($"Projects must have an owner. To change this user's role you must first transfer ownersip to another user.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var membership = user.ProjectMembership(context.Project.Id);

            if (context.User.HasEqualMembership(membership))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships did not change." })
                    .ToActionResult();

            context.User.UpdateProjectMembership(membership);

            var command = new ProjectUserUpdateCommand(context.ContextUser, context.User, context.Project.Id);

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
        public Task<IActionResult> PutMe([FromBody] User user) => ExecuteAsync<TeamCloudProjectContext>(async context =>
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            if (!user.Id.Equals(context.ContextUser.Id, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"User's id does match the id of the current user." })
                    .ToActionResult();

            if (!context.ContextUser.IsMember(context.Project.Id))
                return ErrorResult
                    .NotFound($"A User matching the current user was not found in this Project.")
                    .ToActionResult();

            if (context.ContextUser.IsOwner(context.Project.Id) && !user.IsOwner(context.Project.Id))
                return ErrorResult
                    .BadRequest($"Projects must have an owner. To change this user's role you must first transfer ownersip to another user.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var membership = user.ProjectMembership(context.Project.Id);

            if (context.ContextUser.HasEqualMembership(membership))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships did not change." })
                    .ToActionResult();

            context.ContextUser.UpdateProjectMembership(membership);

            var command = new ProjectUserUpdateCommand(context.ContextUser, context.ContextUser, context.Project.Id);

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
        public Task<IActionResult> Delete([FromRoute] string userId) => ExecuteAsync<TeamCloudProjectUserContext>(async context =>
        {
            if (!context.User.IsMember(context.Project.Id))
                return ErrorResult
                    .NotFound($"The specified User could not be found in this Project.")
                    .ToActionResult();

            if (context.User.IsOwner(context.Project.Id))
                return ErrorResult
                    .BadRequest($"Projects must have an owner. To change this user's role you must first transfer ownersip to another user.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var command = new ProjectUserDeleteCommand(context.ContextUser, context.User, context.Project.Id);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });
    }
}

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
using TeamCloud.API;
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
    [Produces("application/json")]
    public class OrganizationUsersController : ApiController
    {
        public OrganizationUsersController(UserService userService, Orchestrator orchestrator, IOrganizationRepository organizationRepository, IUserRepository userRepository)
            : base(userService, orchestrator, organizationRepository, userRepository)
        { }


        [HttpGet("orgs/{org}/users")]
        [Authorize(Policy = AuthPolicies.UserRead)]
        [SwaggerOperation(OperationId = "GetOrganizationUsers", Summary = "Gets all Users.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Users.", typeof(DataResult<List<User>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The Organization was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => ResolveOrganizationIdAsync(async organizationId =>
        {
            var users = await UserRepository
                .ListAsync(organizationId)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<User>>
                .Ok(users)
                .ToActionResult();
        });


        [HttpGet("orgs/{org}/users/{userNameOrId:userNameOrId}")]
        [Authorize(Policy = AuthPolicies.UserRead)]
        [SwaggerOperation(OperationId = "GetOrganizationUserByNameOrId", Summary = "Gets a User by ID or email address.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns User.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the provided identifier was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Get([FromRoute] string userNameOrId) => EnsureUserAsync(user =>
        {
            return DataResult<User>
                .Ok(user)
                .ToActionResult();
        });

        [HttpGet("orgs/{org}/me")] // TODO: change this to users/orgs (maybe)
        [Authorize(Policy = AuthPolicies.UserRead)]
        [SwaggerOperation(OperationId = "GetOrganizationUserMe", Summary = "Gets a User A User matching the current authenticated user.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns User.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User matching the current user was not found.", typeof(ErrorResult))]
        public Task<IActionResult> GetMe() => EnsureCurrentUserAsync(currentUser =>
        {
            return DataResult<User>
                .Ok(currentUser)
                .ToActionResult();
        });


        [HttpPost("orgs/{org}/users")]
        [Authorize(Policy = AuthPolicies.UserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateOrganizationUser", Summary = "Creates a new User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The Organization was not found, or a User with the email address provided in the request body was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] UserDefinition userDefinition) => ResolveOrganizationIdAsync(async organizationId =>
        {
            if (userDefinition is null)
                throw new ArgumentNullException(nameof(userDefinition));

            var validation = new UserDefinitionTeamCloudValidator().Validate(userDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            var userId = await UserService
                .GetUserIdAsync(userDefinition.Identifier)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(userId))
                return ErrorResult
                    .NotFound($"The user '{userDefinition.Identifier}' could not be found.")
                    .ToActionResult();

            var user = await UserRepository
                .GetAsync(organizationId, userId)
                .ConfigureAwait(false);

            if (user != null)
                return ErrorResult
                    .Conflict($"The user '{userDefinition.Identifier}' already exists on this Organization. Please try your request again with a unique user or call PUT to update the existing User.")
                    .ToActionResult();

            user = new User
            {
                Id = userId,
                Role = Enum.Parse<OrganizationUserRole>(userDefinition.Role, true),
                Properties = userDefinition.Properties,
                UserType = UserType.User
            };

            var currentUser = await UserService
                .CurrentUserAsync(organizationId)
                .ConfigureAwait(false);

            var command = new OrchestratorOrganizationUserCreateCommand(currentUser, user);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPut("orgs/{org}/users/{userNameOrId:userNameOrId}")]
        [Authorize(Policy = AuthPolicies.UserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateOrganizationUser", Summary = "Updates an existing User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Put([FromRoute] string userNameOrId, [FromBody] User user) => EnsureUserAsync(async existingUser =>
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            if (existingUser.IsAdmin() && !user.IsAdmin())
            {
                var otherAdmins = await UserRepository
                    .ListAdminsAsync(OrganizationId)
                    .AnyAsync(a => a.Id != user.Id)
                    .ConfigureAwait(false);

                if (!otherAdmins)
                    return ErrorResult
                        .BadRequest($"The Organization must have at least one Admin user. To change this user's role you must first add another Admin user.", ResultErrorCode.ValidationError)
                        .ToActionResult();
            }

            if (!existingUser.HasEqualMemberships(user))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships can not be changed using the TeamCloud (system) users API. To update a user's project memberships use the project users API." })
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync(OrganizationId)
                .ConfigureAwait(false);

            var command = new OrchestratorOrganizationUserUpdateCommand(currentUser, user);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPut("orgs/{org}/me")] // TODO: change to /users/orgs/{org}
        [Authorize(Policy = AuthPolicies.UserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateOrganizationUserMe", Summary = "Updates an existing User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        public Task<IActionResult> PutMe([FromBody] User user) => EnsureCurrentUserAsync(async currentUser =>
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            if (!currentUser.Id.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"User's id does match the id of the current authenticated user." })
                    .ToActionResult();

            if (currentUser.IsAdmin() && !user.IsAdmin())
            {
                var otherAdmins = await UserRepository
                    .ListAdminsAsync(OrganizationId)
                    .AnyAsync(a => a.Id != user.Id)
                    .ConfigureAwait(false);

                if (!otherAdmins)
                    return ErrorResult
                        .BadRequest($"The Organization must have at least one Admin user. To change this user's role you must first add another Admin user.", ResultErrorCode.ValidationError)
                        .ToActionResult();
            }

            if (!currentUser.HasEqualMemberships(user))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships can not be changed using the TeamCloud (system) users API. To update a user's project memberships use the project users API." })
                    .ToActionResult();

            var command = new OrchestratorOrganizationUserUpdateCommand(currentUser, user);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("orgs/{org}/users/{userNameOrId:userNameOrId}")]
        [Authorize(Policy = AuthPolicies.UserWrite)]
        [SwaggerOperation(OperationId = "DeleteOrganizationUser", Summary = "Deletes an existing User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the identifier provided was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Delete([FromRoute] string userNameOrId) => EnsureUserAsync(async user =>
        {
            if (user.IsAdmin())
            {
                var otherAdmins = await UserRepository
                    .ListAdminsAsync(OrganizationId)
                    .AnyAsync(a => a.Id != user.Id)
                    .ConfigureAwait(false);

                if (!otherAdmins)
                    return ErrorResult
                        .BadRequest($"The Organization must have at least one Admin user. To delete this user you must first add another Admin user.", ResultErrorCode.ValidationError)
                        .ToActionResult();
            }

            var currentUser = await UserService
                .CurrentUserAsync(OrganizationId)
                .ConfigureAwait(false);

            var command = new OrchestratorOrganizationUserDeleteCommand(currentUser, user);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });
    }
}

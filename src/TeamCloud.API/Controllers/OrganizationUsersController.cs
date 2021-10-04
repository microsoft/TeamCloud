/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DotLiquid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API;
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
    [Produces("application/json")]
    public class OrganizationUsersController : TeamCloudController
    {
        private readonly IUserRepository userRepository;

        public OrganizationUsersController(IUserRepository userRepository) : base()
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }


        [HttpGet("orgs/{organizationId:organizationId}/users")]
        [Authorize(Policy = AuthPolicies.OrganizationRead)]
        [SwaggerOperation(OperationId = "GetOrganizationUsers", Summary = "Gets all Users.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Users.", typeof(DataResult<List<User>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The Organization was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => WithContextAsync<Organization>(async (contextUser, organization) =>
        {
            var users = await userRepository
                .ListAsync(organization.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<User>>
                .Ok(users)
                .ToActionResult();
        });


        [HttpGet("orgs/{organizationId:organizationId}/users/{userId:userId}")]
        [Authorize(Policy = AuthPolicies.OrganizationRead)]
        [SwaggerOperation(OperationId = "GetOrganizationUser", Summary = "Gets a User.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns User.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the provided identifier was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Get([FromRoute] string userId) => WithContextAsync<User>((contextUser, user) =>
        {
            return DataResult<User>
                .Ok(user)
                .ToActionResultAsync();
        });

        [HttpGet("orgs/{organizationId:organizationId}/me")] // TODO: change this to users/orgs (maybe)
        [Authorize(Policy = AuthPolicies.OrganizationRead)]
        [SwaggerOperation(OperationId = "GetOrganizationUserMe", Summary = "Gets a User A User matching the current authenticated user.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns User.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User matching the current user was not found.", typeof(ErrorResult))]
        public Task<IActionResult> GetMe() => WithContextAsync(async contextUser =>
        {
            var user = await userRepository
                .ExpandAsync(contextUser, true)
                .ConfigureAwait(false);

            return DataResult<User>
                .Ok(user)
                .ToActionResult();
        });


        [HttpPost("orgs/{organizationId:organizationId}/users")]
        [Authorize(Policy = AuthPolicies.OrganizationUserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateOrganizationUser", Summary = "Creates a new User.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The new Organization User was created.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The Organization was not found, or a User with the email address provided in the request body was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] UserDefinition userDefinition) => WithContextAsync<Organization>(async (contextUser, organization) =>
        {
            if (userDefinition is null)
                throw new ArgumentNullException(nameof(userDefinition));

            var validation = new OrganizationUserDefinitionValidator().Validate(userDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            var newUserId = await UserService
                .GetUserIdAsync(userDefinition.Identifier)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(newUserId))
                return ErrorResult
                    .NotFound($"The user '{userDefinition.Identifier}' could not be found.")
                    .ToActionResult();

            var user = await userRepository
                .GetAsync(organization.Id, newUserId)
                .ConfigureAwait(false);

            if (user != null)
                return ErrorResult
                    .Conflict($"The user '{userDefinition.Identifier}' already exists on this Organization. Please try your request again with a unique user or call PUT to update the existing User.")
                    .ToActionResult();

            user = new User
            {
                Id = newUserId,
                Role = Enum.Parse<OrganizationUserRole>(userDefinition.Role, true),
                Properties = userDefinition.Properties,
                UserType = UserType.User,
                Organization = organization.Id
            };

            var command = new OrganizationUserCreateCommand(contextUser, user);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPut("orgs/{organizationId:organizationId}/users/{userId:userId}")]
        [Authorize(Policy = AuthPolicies.OrganizationUserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateOrganizationUser", Summary = "Updates an existing User.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The user was successfully updated.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Put([FromRoute] string userId, [FromBody] User userUpdate) => WithContextAsync<User>(async (contextUser, user) =>
        {
            if (userUpdate is null)
                throw new ArgumentNullException(nameof(userUpdate));

            var validation = new UserValidator().Validate(userUpdate);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            if (user.IsOwner() && !userUpdate.IsOwner())
                return ErrorResult
                    .BadRequest($"The Organization must have an owner. To change this user's role you must first transfer ownership to another user.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!user.HasEqualMemberships(userUpdate))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships can not be changed using the TeamCloud (system) users API. To update a user's project memberships use the project users API." })
                    .ToActionResult();

            var command = new OrganizationUserUpdateCommand(contextUser, userUpdate);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPut("orgs/{organizationId:organizationId}/me")] // TODO: change to /users/orgs/{org}
        [Authorize(Policy = AuthPolicies.OrganizationUserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateOrganizationUserMe", Summary = "Updates an existing User.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The user was successfully updated.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        public Task<IActionResult> PutMe([FromBody] User user) => WithContextAsync(async contextUser =>
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            if (!contextUser.Id.Equals(user.Id, StringComparison.OrdinalIgnoreCase))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"User's id does match the id of the current authenticated user." })
                    .ToActionResult();

            if (contextUser.IsOwner() && !user.IsOwner())
                return ErrorResult
                    .BadRequest($"The Organization must have an owner. To change this user's role you must first transfer ownership to another user.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!contextUser.HasEqualMemberships(user))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships can not be changed using the TeamCloud (system) users API. To update a user's project memberships use the project users API." })
                    .ToActionResult();

            var command = new OrganizationUserUpdateCommand(contextUser, user);

            var commandResult = (OrganizationUserUpdateCommandResult)await Orchestrator
                .InvokeAsync(command)
                .ConfigureAwait(false);

            // ensure the ME user is expanded
            commandResult.Result = await userRepository
                .ExpandAsync(commandResult.Result, true)
                .ConfigureAwait(false);

            return commandResult.ToActionResult(Request);
        });


        [HttpDelete("orgs/{organizationId:organizationId}/users/{userId:userId}")]
        [Authorize(Policy = AuthPolicies.OrganizationUserWrite)]
        [SwaggerOperation(OperationId = "DeleteOrganizationUser", Summary = "Deletes an existing User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The user was successfully deleted.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the identifier provided was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Delete([FromRoute] string userId) => WithContextAsync<User>(async (contextUser, user) =>
        {
            if (user.IsOwner())
                return ErrorResult
                    .BadRequest($"The Organization must have an owner. To delete this user you must first transfer ownership to another user.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var command = new OrganizationUserDeleteCommand(contextUser, user);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });
    }
}

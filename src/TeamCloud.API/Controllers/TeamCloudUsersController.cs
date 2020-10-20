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
    public class TeamCloudUsersController : ApiController
    {
        public TeamCloudUsersController(UserService userService, Orchestrator orchestrator, IUserRepository userRepository)
            : base(userService, orchestrator, userRepository)
        { }


        [HttpGet("api/users")]
        [Authorize(Policy = AuthPolicies.UserRead)]
        [SwaggerOperation(OperationId = "GetTeamCloudUsers", Summary = "Gets all TeamCloud Users.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all TeamCloud Users.", typeof(DataResult<List<User>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var userDocuments = await UserRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            var users = userDocuments.Select(u => u.PopulateExternalModel()).ToList();

            return DataResult<List<User>>
                .Ok(users)
                .ToActionResult();
        }


        [HttpGet("api/users/{userNameOrId:userNameOrId}")]
        [Authorize(Policy = AuthPolicies.UserRead)]
        [SwaggerOperation(OperationId = "GetTeamCloudUserByNameOrId", Summary = "Gets a TeamCloud User by ID or email address.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns TeamCloud User.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the provided identifier was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Get([FromRoute] string userNameOrId) => EnsureUserAsync(user =>
        {
            return DataResult<User>
                .Ok(user.PopulateExternalModel())
                .ToActionResult();
        });

        [HttpGet("api/me")]
        [Authorize(Policy = AuthPolicies.UserRead)]
        [SwaggerOperation(OperationId = "GetTeamCloudUserMe", Summary = "Gets a TeamCloud User A User matching the current authenticated user.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns TeamCloud User.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User matching the current user was not found.", typeof(ErrorResult))]
        public Task<IActionResult> GetMe() => EnsureCurrentUserAsync(currentUser =>
        {
            return DataResult<User>
                .Ok(currentUser.PopulateExternalModel())
                .ToActionResult();
        });


        [HttpPost("api/users")]
        [Authorize(Policy = AuthPolicies.UserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateTeamCloudUser", Summary = "Creates a new TeamCloud User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new TeamCloud User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a User with the email address provided in the request body was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] UserDefinition userDefinition)
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

            var userDocument = await UserRepository
                .GetAsync(userId)
                .ConfigureAwait(false);

            if (userDocument != null)
                return ErrorResult
                    .Conflict($"The user '{userDefinition.Identifier}' already exists on this TeamCloud Instance. Please try your request again with a unique user or call PUT to update the existing User.")
                    .ToActionResult();

            userDocument = new UserDocument
            {
                Id = userId,
                Role = Enum.Parse<TeamCloudUserRole>(userDefinition.Role, true),
                Properties = userDefinition.Properties,
                UserType = UserType.User
            };

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorTeamCloudUserCreateCommand(currentUser, userDocument);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<UserDocument, User>(command, Request)
                .ConfigureAwait(false);
        }


        [HttpPut("api/users/{userNameOrId:userNameOrId}")]
        [Authorize(Policy = AuthPolicies.UserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateTeamCloudUser", Summary = "Updates an existing TeamCloud User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the TeamCloud User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Put([FromRoute] string userNameOrId, [FromBody] User user) => EnsureUserAsync(async userDocument =>
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            if (userDocument.IsAdmin() && !user.IsAdmin())
            {
                var otherAdmins = await UserRepository
                    .ListAdminsAsync()
                    .AnyAsync(a => a.Id != user.Id)
                    .ConfigureAwait(false);

                if (!otherAdmins)
                    return ErrorResult
                        .BadRequest($"The TeamCloud instance must have at least one Admin user. To change this user's role you must first add another Admin user.", ResultErrorCode.ValidationError)
                        .ToActionResult();
            }

            if (!userDocument.HasEqualMemberships(user))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships can not be changed using the TeamCloud (system) users API. To update a user's project memberships use the project users API." })
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            userDocument.PopulateFromExternalModel(user);

            var command = new OrchestratorTeamCloudUserUpdateCommand(currentUser, userDocument);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<UserDocument, User>(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPut("api/me")]
        [Authorize(Policy = AuthPolicies.UserWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateTeamCloudUserMe", Summary = "Updates an existing TeamCloud User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the TeamCloud User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
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
                    .ListAdminsAsync()
                    .AnyAsync(a => a.Id != user.Id)
                    .ConfigureAwait(false);

                if (!otherAdmins)
                    return ErrorResult
                        .BadRequest($"The TeamCloud instance must have at least one Admin user. To change this user's role you must first add another Admin user.", ResultErrorCode.ValidationError)
                        .ToActionResult();
            }

            if (!currentUser.HasEqualMemberships(user))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships can not be changed using the TeamCloud (system) users API. To update a user's project memberships use the project users API." })
                    .ToActionResult();

            currentUser.PopulateFromExternalModel(user);

            var command = new OrchestratorTeamCloudUserUpdateCommand(currentUser, currentUser);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<UserDocument, User>(command, Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("api/users/{userNameOrId:userNameOrId}")]
        [Authorize(Policy = AuthPolicies.UserWrite)]
        [SwaggerOperation(OperationId = "DeleteTeamCloudUser", Summary = "Deletes an existing TeamCloud User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the TeamCloud User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A User with the identifier provided was not found.", typeof(ErrorResult))]
        [SuppressMessage("Usage", "CA1801: Review unused parameters", Justification = "Used by base class and makes signiture unique")]
        public Task<IActionResult> Delete([FromRoute] string userNameOrId) => EnsureUserAsync(async userDocument =>
        {
            if (userDocument.IsAdmin())
            {
                var otherAdmins = await UserRepository
                    .ListAdminsAsync()
                    .AnyAsync(a => a.Id != userDocument.Id)
                    .ConfigureAwait(false);

                if (!otherAdmins)
                    return ErrorResult
                        .BadRequest($"The TeamCloud instance must have at least one Admin user. To delete this user you must first add another Admin user.", ResultErrorCode.ValidationError)
                        .ToActionResult();
            }

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorTeamCloudUserDeleteCommand(currentUser, userDocument);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<UserDocument, User>(command, Request)
                .ConfigureAwait(false);
        });
    }
}

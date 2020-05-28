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
using TeamCloud.Model.Validation.Data;

namespace TeamCloud.API
{
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    public class TeamCloudUsersController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly ITeamCloudRepository teamCloudRepository;
        readonly IUsersRepository usersRepository;

        public TeamCloudUsersController(UserService userService, Orchestrator orchestrator, ITeamCloudRepository teamCloudRepository, IUsersRepository usersRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }


        [HttpGet]
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "GetTeamCloudUsers", Summary = "Gets all TeamCloud Users.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all TeamCloud Users.", typeof(DataResult<List<User>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var users = await usersRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            return DataResult<List<User>>
                .Ok(users)
                .ActionResult();
        }


        [HttpGet("{userNameOrId:userNameOrId}")]
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "GetTeamCloudUserByNameOrId", Summary = "Gets a TeamCloud User by ID or email address.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns TeamCloud User.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a User with the provided identifier was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get([FromRoute] string userNameOrId)
        {
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

            if (user is null)
                return ErrorResult
                    .NotFound($"The specified User could not be found in this TeamCloud Instance.")
                    .ActionResult();

            return DataResult<User>
                .Ok(user)
                .ActionResult();
        }


        [HttpPost]
        [Authorize(Policy = "admin")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateTeamCloudUser", Summary = "Creates a new TeamCloud User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new TeamCloud User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a User with the email address provided in the request body was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A TeamCloud User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] UserDefinition userDefinition)
        {
            if (userDefinition is null)
                throw new ArgumentNullException(nameof(userDefinition));

            var validation = new TeamCloudUserDefinitionValidator().Validate(userDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var userId = await userService
                .GetUserIdAsync(userDefinition.Identifier)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(userId))
                return ErrorResult
                    .NotFound($"The user '{userDefinition.Identifier}' could not be found.")
                    .ActionResult();

            var user = await usersRepository
                .GetAsync(userId)
                .ConfigureAwait(false);

            if (user != null)
                return ErrorResult
                    .Conflict($"The user '{userDefinition.Identifier}' already exists on this TeamCloud Instance. Please try your request again with a unique user or call PUT to update the existing User.")
                    .ActionResult();

            user = new User
            {
                Id = userId,
                Role = Enum.Parse<TeamCloudUserRole>(userDefinition.Role, true),
                Properties = userDefinition.Properties,
                UserType = UserType.User
            };

            var currentUserForCommand = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorTeamCloudUserCreateCommand(currentUserForCommand, user);

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
        [Authorize(Policy = "admin")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateTeamCloudUser", Summary = "Updates an existing TeamCloud User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the TeamCloud User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromBody] User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var oldUser = await usersRepository
                .GetAsync(user.Id)
                .ConfigureAwait(false);

            if (oldUser is null)
                return ErrorResult
                    .NotFound($"The user '{oldUser.Id}' could not be found on this TeamCloud Instance.")
                    .ActionResult();

            if (oldUser.IsAdmin() && !user.IsAdmin())
            {
                var otherAdmins = await usersRepository
                    .ListAdminsAsync()
                    .AnyAsync(a => a.Id != user.Id)
                    .ConfigureAwait(false);

                if (!otherAdmins)
                    return ErrorResult
                        .BadRequest($"The TeamCloud instance must have at least one Admin user. To change this user's role you must first add another Admin user.", ResultErrorCode.ValidationError)
                        .ActionResult();
            }

            if (!oldUser.HasEqualMemberships(user))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectMemberships", Message = $"User's project memberships can not be changed using the TeamCloud (system) users API. To update a user's project memberships use the project users API." })
                    .ActionResult();

            var currentUserForCommand = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorTeamCloudUserUpdateCommand(currentUserForCommand, user);

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
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "DeleteTeamCloudUser", Summary = "Deletes an existing TeamCloud User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the TeamCloud User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a User with the identifier provided was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete([FromRoute] string userNameOrId)
        {
            if (string.IsNullOrWhiteSpace(userNameOrId))
                return ErrorResult
                    .BadRequest($"The identifier '{userNameOrId}' provided in the url path is invalid.  Must be a valid email address or GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
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

            if (user is null)
                return ErrorResult
                    .NotFound($"The specified User could not be found in this TeamCloud Instance.")
                    .ActionResult();

            if (user.IsAdmin())
            {
                var otherAdmins = await usersRepository
                    .ListAdminsAsync()
                    .AnyAsync(a => a.Id != user.Id)
                    .ConfigureAwait(false);

                if (!otherAdmins)
                    return ErrorResult
                        .BadRequest($"The TeamCloud instance must have at least one Admin user. To delete this user you must first add another Admin user.", ResultErrorCode.ValidationError)
                        .ActionResult();
            }

            var currentUserForCommand = await userService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorTeamCloudUserDeleteCommand(currentUserForCommand, user);

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

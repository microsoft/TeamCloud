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

namespace TeamCloud.API
{
    [ApiController]
    [Route("api/users")]
    [Produces("application/json")]
    public class TeamCloudUsersController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly ITeamCloudRepositoryReadOnly teamCloudRepository;

        public TeamCloudUsersController(UserService userService, Orchestrator orchestrator, ITeamCloudRepositoryReadOnly teamCloudRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        private User CurrentUser => new User()
        {
            Id = userService.CurrentUserId,
            Role = UserRoles.Project.Owner
        };


        [HttpGet]
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "GetTeamCloudUsers", Summary = "Gets all TeamCloud Users.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all TeamCloud Users.", typeof(DataResult<List<User>>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            var users = teamCloudInstance?.Users ?? new List<User>();

            return DataResult<List<User>>
                .Ok(users.ToList())
                .ActionResult();
        }


        [HttpGet("{userNameOrId:userNameOrId}")]
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "GetTeamCloudUserByNameOrId", Summary = "Gets a TeamCloud User by ID or email address.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns TeamCloud User.", typeof(DataResult<User>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a User with the provided identifier was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get([FromRoute] string userNameOrId)
        {
            if (string.IsNullOrWhiteSpace(userNameOrId))
                return ErrorResult
                    .BadRequest($"The identifier '{userNameOrId}' provided in the url path is invalid.  Must be a valid email address or GUID.", ResultErrorCodes.ValidationError)
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
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

            var user = teamCloudInstance.Users?.FirstOrDefault(u => u.Id == userId);

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
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The User provided in the request body did not pass validation.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a User with the email address provided in the request body was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A TeamCloud User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] UserDefinition userDefinition)
        {
            var validation = new UserDefinitionValidator().Validate(userDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            var newUser = await userService
                .GetUserAsync(userDefinition)
                .ConfigureAwait(false);

            if (newUser is null)
                return ErrorResult
                    .NotFound($"A User with the Email '{userDefinition.Email}' could not be found.")
                    .ActionResult();

            if (teamCloudInstance.Users.Contains(newUser))
                return ErrorResult
                    .Conflict($"A User with the Email '{userDefinition.Email}' already exists on this TeamCloud Instance. Please try your request again with a unique email or call PUT to update the existing User.")
                    .ActionResult();

            var command = new OrchestratorTeamCloudUserCreateCommand(CurrentUser, newUser);

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
        [Authorize(Policy = "admin")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateTeamCloudUser", Summary = "Updates an existing TeamCloud User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the TeamCloud User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The User provided in the request body did not pass validation.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromBody] User user)
        {
            var validation = new UserValidator().Validate(user);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            var oldUser = teamCloudInstance.Users?.FirstOrDefault(u => u.Id == user.Id);

            if (oldUser is null)
                return ErrorResult
                    .NotFound($"A User with the ID '{oldUser.Id}' could not be found on this TeamCloud Instance.")
                    .ActionResult();

            var command = new OrchestratorTeamCloudUserUpdateCommand(CurrentUser, user);

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
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "DeleteTeamCloudUser", Summary = "Deletes an existing TeamCloud User.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the TeamCloud User. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a User with the identifier provided was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete([FromRoute] string userNameOrId)
        {
            if (string.IsNullOrWhiteSpace(userNameOrId))
                return ErrorResult
                    .BadRequest($"The identifier '{userNameOrId}' provided in the url path is invalid.  Must be a valid email address or GUID.", ResultErrorCodes.ValidationError)
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
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

            var user = teamCloudInstance.Users?.FirstOrDefault(u => u.Id == userId);

            if (user is null)
                return ErrorResult
                    .NotFound($"The specified User could not be found in this TeamCloud Instance.")
                    .ActionResult();

            var command = new OrchestratorTeamCloudUserDeleteCommand(CurrentUser, user);

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

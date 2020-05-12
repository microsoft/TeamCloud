/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
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
    public class TeamCloudAdminController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly IUsersRepositoryReadOnly usersRepository;

        public TeamCloudAdminController(UserService userService, Orchestrator orchestrator, IUsersRepositoryReadOnly usersRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
        }


        [HttpPost("api/admin/users")]
        [Authorize(Policy = "default")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateTeamCloudAdminUser", Summary = "Creates a new TeamCloud User as an Admin.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new TeamCloud User as an Admin. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The User provided in the request body did not pass validation.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a User with the email address provided in the request body was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A TeamCloud User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] UserDefinition userDefinition)
        {
            if (userDefinition is null)
                throw new ArgumentNullException(nameof(userDefinition));

            var validation = new TeamCloudUserDefinitionAdminValidator().Validate(userDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var users = await usersRepository
                .ListAdminsAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            if (users.Any())
                return ErrorResult
                    .BadRequest($"The TeamCloud instance already has one or more Admin users. To add additional users to the TeamCloud instance POST to api/users.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var newUser = await userService
                .ResolveUserAsync(userDefinition)
                .ConfigureAwait(false);

            if (newUser is null)
                return ErrorResult
                    .NotFound($"A User with the Email '{userDefinition.Email}' could not be found.")
                    .ActionResult();

            newUser.Role = TeamCloudUserRole.Admin;

            // var currentUserForCommand = await userService
            //     .CurrentUserAsync()
            //     .ConfigureAwait(false);

            // var command = new OrchestratorTeamCloudUserCreateCommand(currentUserForCommand, newUser);

            var command = new OrchestratorTeamCloudUserCreateCommand(newUser, newUser);

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

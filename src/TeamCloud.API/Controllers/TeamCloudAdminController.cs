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
using TeamCloud.API.Auth;
using TeamCloud.API.Data;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Data.Validators;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    public class TeamCloudAdminController : ApiController
    {
        private readonly IUserRepository usersRepository;
        private readonly ITeamCloudRepository teamCloudRepository;

        public TeamCloudAdminController(UserService userService, Orchestrator orchestrator, IUserRepository usersRepository, ITeamCloudRepository teamCloudRepository) : base(userService, orchestrator)
        {
            this.usersRepository = usersRepository ?? throw new ArgumentNullException(nameof(usersRepository));
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }


        [HttpPost("api/admin/users")]
        [Authorize(Policy = AuthPolicies.Default)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateTeamCloudAdminUser", Summary = "Creates a new TeamCloud User as an Admin.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new TeamCloud User as an Admin. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a User with the email address provided in the request body was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A TeamCloud User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] UserDefinition userDefinition)
        {
            if (userDefinition is null)
                throw new ArgumentNullException(nameof(userDefinition));

            var validation = new UserDefinitionTeamCloudAdminValidator().Validate(userDefinition);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            var adminUsers = await usersRepository
                .ListAdminsAsync()
                .AnyAsync()
                .ConfigureAwait(false);

            if (adminUsers)
                return ErrorResult
                    .BadRequest($"The TeamCloud instance already has one or more Admin users. To add additional users to the TeamCloud instance POST to api/users.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var userId = await UserService
                .GetUserIdAsync(userDefinition.Identifier)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(userId))
                return ErrorResult
                    .NotFound($"The user '{userDefinition.Identifier}' could not be found.")
                    .ToActionResult();

            var user = new UserDocument
            {
                Id = userId,
                Role = Enum.Parse<TeamCloudUserRole>(userDefinition.Role, true),
                Properties = userDefinition.Properties,
                UserType = UserType.User
            };

            // no users exist in the database yet and the cli calls this api implicitly immediatly
            // after the teamcloud instance is created to add the instance creator as an admin user
            // thus, we can assume the calling user and the user from the payload are the same
            var command = new OrchestratorTeamCloudUserCreateCommand(user, user);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<UserDocument, User>(command, Request)
                .ConfigureAwait(false);
        }


        [HttpGet("api/admin/teamCloudInstance")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "GetTeamCloudInstance", Summary = "Gets the TeamCloud instance.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns the TeamCloudInstance.", typeof(DataResult<TeamCloudInstance>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"The TeamCloud instance could not be found.")
                    .ToActionResult();

            var returnTeamCloudInstance = teamCloudInstance.PopulateExternalModel();

            return DataResult<TeamCloudInstance>
                .Ok(returnTeamCloudInstance)
                .ToActionResult();
        }


        [HttpPost("api/admin/teamCloudInstance")]
        [Authorize(Policy = AuthPolicies.Default)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateTeamCloudInstance", Summary = "Updates the TeamCloud instance.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The TeamCloud instance was created.", typeof(DataResult<TeamCloudInstance>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] TeamCloudInstance teamCloudInstance)
        {
            if (teamCloudInstance is null)
            {
                return ErrorResult
                    .BadRequest("Request body must not be empty.", ResultErrorCode.ValidationError)
                    .ToActionResult();
            }
            else if (!teamCloudInstance.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
            {
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();
            }

            var teamCloudInstanceDocument = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstanceDocument is null)
            {
                return ErrorResult
                    .NotFound("The TeamCloud instance could not be found.")
                    .ToActionResult();
            }

            if (teamCloudInstanceDocument.ResourceGroup != null
                || teamCloudInstanceDocument.Version != null
                || (teamCloudInstanceDocument.Tags?.Any() ?? false))
            {
                return ErrorResult
                    .Conflict($"The TeamCloud instance already exists.  Call PUT to update the existing instance.")
                    .ToActionResult();
            }

            teamCloudInstanceDocument.Version = teamCloudInstance.Version;
            teamCloudInstanceDocument.ResourceGroup = teamCloudInstance.ResourceGroup;
            teamCloudInstanceDocument.Tags = teamCloudInstance.Tags;

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<TeamCloudInstanceDocument, TeamCloudInstance>(new OrchestratorTeamCloudInstanceSetCommand(currentUser, teamCloudInstanceDocument), Request)
                .ConfigureAwait(false);
        }


        [HttpPut("api/admin/teamCloudInstance")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateTeamCloudInstance", Summary = "Updates the TeamCloud instance.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The TeamCloud instance was updated.", typeof(DataResult<TeamCloudInstance>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromBody] TeamCloudInstance teamCloudInstance)
        {
            if (teamCloudInstance is null)
            {
                return ErrorResult
                    .BadRequest("Request body must not be empty.", ResultErrorCode.ValidationError)
                    .ToActionResult();
            }
            else if (!teamCloudInstance.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
            {
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();
            }

            var teamCloudInstanceDocument = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstanceDocument is null)
                return ErrorResult
                    .NotFound("The TeamCloud instance could not be found.")
                    .ToActionResult();

            if (!string.IsNullOrEmpty(teamCloudInstance.Version))
                teamCloudInstanceDocument.Version = teamCloudInstance.Version;

            if (!(teamCloudInstance.ResourceGroup is null))
                teamCloudInstanceDocument.ResourceGroup = teamCloudInstance.ResourceGroup;

            if (teamCloudInstance.Tags?.Any() ?? false)
                teamCloudInstanceDocument.MergeTags(teamCloudInstance.Tags);

            if (teamCloudInstance.Applications?.Any() ?? false)
                teamCloudInstanceDocument.Applications = teamCloudInstance.Applications;

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<TeamCloudInstanceDocument, TeamCloudInstance>(new OrchestratorTeamCloudInstanceSetCommand(currentUser, teamCloudInstanceDocument), Request)
                .ConfigureAwait(false);
        }
    }
}

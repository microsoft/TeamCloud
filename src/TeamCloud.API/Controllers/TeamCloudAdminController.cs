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
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    public class TeamCloudAdminController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly IUsersRepository usersRepository;
        readonly ITeamCloudRepository teamCloudRepository;

        public TeamCloudAdminController(UserService userService, Orchestrator orchestrator, IUsersRepository usersRepository, ITeamCloudRepository teamCloudRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
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
                    .ActionResult();

            var adminUsers = await usersRepository
                .ListAdminsAsync()
                .AnyAsync()
                .ConfigureAwait(false);

            if (adminUsers)
                return ErrorResult
                    .BadRequest($"The TeamCloud instance already has one or more Admin users. To add additional users to the TeamCloud instance POST to api/users.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var userId = await userService
                .GetUserIdAsync(userDefinition.Identifier)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(userId))
                return ErrorResult
                    .NotFound($"The user '{userDefinition.Identifier}' could not be found.")
                    .ActionResult();

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

            return await orchestrator
                .InvokeAndReturnAccepted(command)
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
                    .ActionResult();

            var returnTeamCloudInstance = teamCloudInstance.PopulateExternalModel();

            return DataResult<TeamCloudInstance>
                .Ok(returnTeamCloudInstance)
                .ActionResult();
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
                throw new ArgumentNullException(nameof(teamCloudInstance));

            var validation = new TeamCloudInstanceValidaor().Validate(teamCloudInstance);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var existingTeamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (existingTeamCloudInstance is null)
                return ErrorResult
                    .NotFound("The TeamCloud instance could not be found.")
                    .ActionResult();

            if (existingTeamCloudInstance.ResourceGroup != null
             || existingTeamCloudInstance.Version != null
             || (existingTeamCloudInstance.Tags?.Any() ?? false))
                return ErrorResult
                    .Conflict($"The TeamCloud instance already exists.  Call PUT to update the existing instance.")
                    .ActionResult();

            existingTeamCloudInstance.Version = teamCloudInstance.Version;
            existingTeamCloudInstance.ResourceGroup = teamCloudInstance.ResourceGroup;
            existingTeamCloudInstance.Tags = teamCloudInstance.Tags;

            var setResult = await orchestrator
                .SetAsync(existingTeamCloudInstance)
                .ConfigureAwait(false);

            var baseUrl = HttpContext.GetApplicationBaseUrl();
            var location = new Uri(baseUrl, $"api/admin/teamCloudInstance").ToString();

            var returnSetResult = setResult.PopulateExternalModel();

            return DataResult<TeamCloudInstance>
                .Created(returnSetResult, location)
                .ActionResult();
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
                throw new ArgumentNullException(nameof(teamCloudInstance));

            var validation = new TeamCloudInstanceValidaor().Validate(teamCloudInstance);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var existingTeamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (existingTeamCloudInstance is null)
                return ErrorResult
                    .NotFound("The TeamCloud instance could not be found.")
                    .ActionResult();

            if (!string.IsNullOrEmpty(teamCloudInstance.Version))
                existingTeamCloudInstance.Version = teamCloudInstance.Version;

            if (!(teamCloudInstance.ResourceGroup is null))
                existingTeamCloudInstance.ResourceGroup = teamCloudInstance.ResourceGroup;

            if (teamCloudInstance.Tags?.Any() ?? false)
                existingTeamCloudInstance.MergeTags(teamCloudInstance.Tags);

            var setResult = await orchestrator
                .SetAsync(existingTeamCloudInstance)
                .ConfigureAwait(false);

            var returnSetResult = setResult.PopulateExternalModel();

            return DataResult<TeamCloudInstance>
                .Ok(returnSetResult)
                .ActionResult();
        }
    }
}

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
using TeamCloud.Model.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/tags")]
    [Produces("application/json")]
    public class TeamCloudTagsController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly ITeamCloudRepositoryReadOnly teamCloudRepository;

        public TeamCloudTagsController(UserService userService, Orchestrator orchestrator, ITeamCloudRepositoryReadOnly teamCloudRepository)
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
        [SwaggerOperation(OperationId = "GetTeamCloudTags", Summary = "Gets all Tags for a TeamCloud Instance.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all TeamCloud Tags", typeof(DataResult<Dictionary<string, string>>))]
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

            var tags = teamCloudInstance?.Tags is null ? new Dictionary<string, string>() : new Dictionary<string, string>(teamCloudInstance.Tags);

            return DataResult<Dictionary<string, string>>
                .Ok(tags)
                .ActionResult();
        }


        [HttpGet("{tagKey}")]
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "GetTeamCloudTagByKey", Summary = "Gets a TeamCloud Tag by Key.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns TeamCloud Tag", typeof(DataResult<Dictionary<string, string>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The key provided in the path was invalid.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a Tag with the provided key was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get([FromRoute] string tagKey)
        {
            if (string.IsNullOrWhiteSpace(tagKey))
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            if (!teamCloudInstance.Tags.TryGetValue(tagKey, out var tagValue))
                return ErrorResult
                    .NotFound($"The specified Tag could not be found in this TeamCloud Instance.")
                    .ActionResult();

            return DataResult<Dictionary<string, string>>
                .Ok(new Dictionary<string, string> { { tagKey, tagValue } })
                .ActionResult();
        }


        [HttpPost]
        [Authorize(Policy = "admin")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateTeamCloudTag", Summary = "Creates a new TeamCloud Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new TeamCloud Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The key provided in the request body was invalid.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A TeamCloud Tag already exists with the key provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] Dictionary<string, string> tags)
        {
            var tag = tags.FirstOrDefault();

            if (tag.Key is null)
                return ErrorResult
                    .BadRequest()
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            if (teamCloudInstance.Tags.ContainsKey(tag.Key))
                return ErrorResult
                    .Conflict($"A Tag with the key '{tag.Key}' already exists on this TeamCloud Instance. Please try your request again with a unique key or call PUT to update the existing Tag.")
                    .ActionResult();

            // TODO:
            return new OkResult();
            // var command = new ProjectUserCreateCommand(CurrentUser, newUser, ProjectId.Value);

            // var commandResult = await orchestrator
            //     .InvokeAsync(command)
            //     .ConfigureAwait(false);

            // if (commandResult.Links.TryGetValue("status", out var statusUrl))
            //     return StatusResult
            //         .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
            //         .ActionResult();

            throw new Exception("This shouldn't happen, but we need to decide to do when it does.");
        }


        [HttpPut]
        [Authorize(Policy = "admin")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateTeamCloudTag", Summary = "Updates an existing TeamCloud Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the TeamCloud Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The Tag provided in the request body did not pass validation.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a Tag with the key provided in the request body was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromBody] Dictionary<string, string> tags)
        {
            var tag = tags.FirstOrDefault();

            if (tag.Key is null)
                return ErrorResult
                    .BadRequest()
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();

            if (!teamCloudInstance.Tags.ContainsKey(tag.Key))
                return ErrorResult
                    .NotFound($"A Tag with the key '{tag.Key}' could not be found in this TeamCloud Instance.")
                    .ActionResult();


            // TODO:
            return new OkResult();
            // var command = new ProjectUserUpdateCommand(CurrentUser, user, ProjectId.Value);

            // var commandResult = await orchestrator
            //     .InvokeAsync(command)
            //     .ConfigureAwait(false);

            // if (commandResult.Links.TryGetValue("status", out var statusUrl))
            //     return StatusResult
            //         .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
            //         .ActionResult();

            throw new Exception("This shouldn't happen, but we need to decide to do when it does.");
        }


        [HttpDelete("{tagKey}")]
        [Authorize(Policy = "admin")]
        [SwaggerOperation(OperationId = "DeleteTeamCloudTag", Summary = "Deletes an existing TeamCloud Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the TeamCloud Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The key provided in the path was invalid.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a Tag with the provided key was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete([FromRoute]string tagKey)
        {
            if (string.IsNullOrWhiteSpace(tagKey))
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ActionResult();
            if (!teamCloudInstance.Tags.TryGetValue(tagKey, out _))
                return ErrorResult
                    .NotFound($"The specified Tag could not be found in this TeamCloud Instance.")
                    .ActionResult();

            // TODO:
            return new NoContentResult();
            // var command = new ProjectUserDeleteCommand(CurrentUser, user, ProjectId.Value);

            // var commandResult = await orchestrator
            //     .InvokeAsync(command)
            //     .ConfigureAwait(false);

            // if (commandResult.Links.TryGetValue("status", out var statusUrl))
            //     return StatusResult
            //         .Accepted(commandResult.CommandId.ToString(), statusUrl, commandResult.RuntimeStatus.ToString(), commandResult.CustomStatus)
            //         .ActionResult();

            throw new Exception("This shouldn't happen, but we need to decide to do when it does.");
        }
    }
}

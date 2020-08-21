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
using TeamCloud.API.Auth;
using TeamCloud.API.Data.Results;
using TeamCloud.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/tags")]
    [Produces("application/json")]
    public class TeamCloudTagsController : ApiController
    {
        readonly ITeamCloudRepository teamCloudRepository;

        public TeamCloudTagsController(ITeamCloudRepository teamCloudRepository)
        {
            this.teamCloudRepository = teamCloudRepository ?? throw new ArgumentNullException(nameof(teamCloudRepository));
        }

        [HttpGet]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "GetTeamCloudTags", Summary = "Gets all Tags for a TeamCloud Instance.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all TeamCloud Tags", typeof(DataResult<Dictionary<string, string>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ToActionResult();

            var tags = teamCloudInstance?.Tags is null ? new Dictionary<string, string>() : new Dictionary<string, string>(teamCloudInstance.Tags);

            return DataResult<Dictionary<string, string>>
                .Ok(tags)
                .ToActionResult();
        }


        [HttpGet("{tagKey}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "GetTeamCloudTagByKey", Summary = "Gets a TeamCloud Tag by Key.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns TeamCloud Tag", typeof(DataResult<Dictionary<string, string>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a Tag with the provided key was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get([FromRoute] string tagKey)
        {
            if (string.IsNullOrWhiteSpace(tagKey))
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ToActionResult();

            if (!teamCloudInstance.Tags.TryGetValue(tagKey, out var tagValue))
                return ErrorResult
                    .NotFound($"The specified Tag could not be found in this TeamCloud Instance.")
                    .ToActionResult();

            return DataResult<Dictionary<string, string>>
                .Ok(new Dictionary<string, string> { { tagKey, tagValue } })
                .ToActionResult();
        }


        [HttpPost]
        [Authorize(Policy = AuthPolicies.Admin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateTeamCloudTag", Summary = "Creates a new TeamCloud Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new TeamCloud Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A TeamCloud Tag already exists with the key provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] Dictionary<string, string> tags)
        {
            var tag = tags.FirstOrDefault();

            if (tag.Key is null)
                return ErrorResult
                    .BadRequest()
                    .ToActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ToActionResult();

            if (teamCloudInstance.Tags.ContainsKey(tag.Key))
                return ErrorResult
                    .Conflict($"A Tag with the key '{tag.Key}' already exists on this TeamCloud Instance. Please try your request again with a unique key or call PUT to update the existing Tag.")
                    .ToActionResult();

            // TODO:
            return new OkResult();
            // var command = new ProjectUserCreateCommand(CurrentUser, newUser, ProjectId.Value);

            // return await orchestrator
            //     .InvokeAndReturnAccepted(command)
            //     .ConfigureAwait(false);
        }


        [HttpPut]
        [Authorize(Policy = AuthPolicies.Admin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateTeamCloudTag", Summary = "Updates an existing TeamCloud Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the TeamCloud Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a Tag with the key provided in the request body was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromBody] Dictionary<string, string> tags)
        {
            var tag = tags.FirstOrDefault();

            if (tag.Key is null)
                return ErrorResult
                    .BadRequest()
                    .ToActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ToActionResult();

            if (!teamCloudInstance.Tags.ContainsKey(tag.Key))
                return ErrorResult
                    .NotFound($"A Tag with the key '{tag.Key}' could not be found in this TeamCloud Instance.")
                    .ToActionResult();


            // TODO:
            return new OkResult();
            // var command = new ProjectUserUpdateCommand(CurrentUser, user, ProjectId.Value);

            // return await orchestrator
            //     .InvokeAndReturnAccepted(command)
            //     .ConfigureAwait(false);
        }


        [HttpDelete("{tagKey}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "DeleteTeamCloudTag", Summary = "Deletes an existing TeamCloud Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the TeamCloud Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "The TeamCloud instance was not found, or a Tag with the provided key was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete([FromRoute] string tagKey)
        {
            if (string.IsNullOrWhiteSpace(tagKey))
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var teamCloudInstance = await teamCloudRepository
                .GetAsync()
                .ConfigureAwait(false);

            if (teamCloudInstance is null)
                return ErrorResult
                    .NotFound($"No TeamCloud Instance was found.")
                    .ToActionResult();
            if (!teamCloudInstance.Tags.TryGetValue(tagKey, out _))
                return ErrorResult
                    .NotFound($"The specified Tag could not be found in this TeamCloud Instance.")
                    .ToActionResult();

            // TODO:
            return new NoContentResult();
            // var command = new ProjectUserDeleteCommand(CurrentUser, user, ProjectId.Value);

            // return await orchestrator
            //     .InvokeAndReturnAccepted(command)
            //     .ConfigureAwait(false);
        }
    }
}

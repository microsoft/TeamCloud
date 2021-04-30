/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Controllers.Core;
using TeamCloud.API.Data.Results;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{organizationId:organizationId}/projects/{projectId:projectId}/tags")]
    [Produces("application/json")]
    public class ProjectTagsController : TeamCloudController
    {
        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetProjectTags", Summary = "Gets all Tags for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Tags", typeof(DataResult<Dictionary<string, string>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => ExecuteAsync<TeamCloudProjectContext>(context =>
        {
            var tags = context.Project?.Tags is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(context.Project.Tags);

            return DataResult<Dictionary<string, string>>
                .Ok(tags)
                .ToActionResultAsync();
        });


        [HttpGet("{tagKey}")]
        [Authorize(Policy = AuthPolicies.ProjectMember)]
        [SwaggerOperation(OperationId = "GetProjectTagByKey", Summary = "Gets a Project Tag by Key.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project Tag", typeof(DataResult<Dictionary<string, string>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the provided key was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string tagKey) => ExecuteAsync<TeamCloudProjectContext>(context =>
        {
            if (string.IsNullOrWhiteSpace(tagKey))
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResultAsync();

            if (!context.Project.Tags.TryGetValue(tagKey, out var tagValue))
                return ErrorResult
                    .NotFound($"The specified Tag could not be found in this Project.")
                    .ToActionResultAsync();

            return DataResult<Dictionary<string, string>>
                .Ok(new Dictionary<string, string> { { tagKey, tagValue } })
                .ToActionResultAsync();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectAdmin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectTag", Summary = "Creates a new Project Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Project Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Tag already exists with the key provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] Dictionary<string, string> tags) => ExecuteAsync<TeamCloudProjectContext>(context =>
        {
            var tag = tags.FirstOrDefault();

            if (tag.Key is null)
                return ErrorResult
                    .BadRequest()
                    .ToActionResultAsync();

            if (context.Project.Tags.ContainsKey(tag.Key))
                return ErrorResult
                    .Conflict($"A Tag with the key '{tag.Key}' already exists on this Project. Please try your request again with a unique key or call PUT to update the existing Tag.")
                    .ToActionResultAsync();

            // TODO:
            return Task.FromResult<IActionResult>(new OkResult());
            // var command = new ProjectUserCreateCommand(CurrentUser, newUser, projectId);

            // return await orchestrator
            //     .InvokeAndReturnAccepted(command)
            //     .ConfigureAwait(false);
        });


        [HttpPut]
        [Authorize(Policy = AuthPolicies.ProjectAdmin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectTag", Summary = "Updates an existing Project Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the key provided in the request body was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Put([FromBody] Dictionary<string, string> tags) => ExecuteAsync<TeamCloudProjectContext>(context =>
        {
            var tag = tags.FirstOrDefault();

            if (tag.Key is null)
                return ErrorResult
                    .BadRequest()
                    .ToActionResultAsync();

            if (!context.Project.Tags.ContainsKey(tag.Key))
                return ErrorResult
                    .NotFound($"A Tag with the key '{tag.Key}' could not be found in this Project.")
                    .ToActionResultAsync();

            // TODO:
            return Task.FromResult<IActionResult>(new OkResult());
            // var command = new ProjectUserUpdateCommand(CurrentUser, user, projectId);

            // return await orchestrator
            //     .InvokeAndReturnAccepted(command)
            //     .ConfigureAwait(false);
        });


        [HttpDelete("{tagKey}")]
        [Authorize(Policy = AuthPolicies.ProjectAdmin)]
        [SwaggerOperation(OperationId = "DeleteProjectTag", Summary = "Deletes an existing Project Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the provided key was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Delete([FromRoute] string tagKey) => ExecuteAsync<TeamCloudProjectContext>(context =>
        {
            if (string.IsNullOrWhiteSpace(tagKey))
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResultAsync();

            if (!context.Project.Tags.TryGetValue(tagKey, out _))
                return ErrorResult
                    .NotFound($"The specified Tag could not be found in this Project.")
                    .ToActionResultAsync();

            // TODO:
            return Task.FromResult<IActionResult>(new NoContentResult());
            // var command = new ProjectUserDeleteCommand(CurrentUser, user, projectId);

            // return await orchestrator
            //     .InvokeAndReturnAccepted(command)
            //     .ConfigureAwait(false);
        });
    }
}

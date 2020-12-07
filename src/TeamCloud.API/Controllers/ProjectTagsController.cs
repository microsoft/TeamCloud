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
using TeamCloud.API.Data.Results;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("orgs/{org}/projects/{projectId:projectId}/tags")]
    [Produces("application/json")]
    public class ProjectTagsController : ApiController
    {
        public ProjectTagsController() : base()
        { }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectTags", Summary = "Gets all Tags for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Tags", typeof(DataResult<Dictionary<string, string>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => EnsureProjectAsync(project =>
        {
            var tags = project?.Tags is null ? new Dictionary<string, string>() : new Dictionary<string, string>(project.Tags);

            return DataResult<Dictionary<string, string>>
                .Ok(tags)
                .ToActionResult();
        });


        [HttpGet("{tagKey}")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectTagByKey", Summary = "Gets a Project Tag by Key.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project Tag", typeof(DataResult<Dictionary<string, string>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the provided key was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string tagKey) => EnsureProjectAsync(project =>
        {
            if (string.IsNullOrWhiteSpace(tagKey))
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!project.Tags.TryGetValue(tagKey, out var tagValue))
                return ErrorResult
                    .NotFound($"The specified Tag could not be found in this Project.")
                    .ToActionResult();

            return DataResult<Dictionary<string, string>>
                .Ok(new Dictionary<string, string> { { tagKey, tagValue } })
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectTag", Summary = "Creates a new Project Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Project Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Tag already exists with the key provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] Dictionary<string, string> tags) => EnsureProjectAsync(project =>
        {
            var tag = tags.FirstOrDefault();

            if (tag.Key is null)
                return ErrorResult
                    .BadRequest()
                    .ToActionResult();

            if (project.Tags.ContainsKey(tag.Key))
                return ErrorResult
                    .Conflict($"A Tag with the key '{tag.Key}' already exists on this Project. Please try your request again with a unique key or call PUT to update the existing Tag.")
                    .ToActionResult();

            // TODO:
            return new OkResult();
            // var command = new ProjectUserCreateCommand(CurrentUser, newUser, projectId);

            // return await orchestrator
            //     .InvokeAndReturnAccepted(command)
            //     .ConfigureAwait(false);
        });


        [HttpPut]
        [Authorize(Policy = AuthPolicies.ProjectWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectTag", Summary = "Updates an existing Project Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the key provided in the request body was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Put([FromBody] Dictionary<string, string> tags) => EnsureProjectAsync(project =>
        {
            var tag = tags.FirstOrDefault();

            if (tag.Key is null)
                return ErrorResult
                    .BadRequest()
                    .ToActionResult();

            if (!project.Tags.ContainsKey(tag.Key))
                return ErrorResult
                    .NotFound($"A Tag with the key '{tag.Key}' could not be found in this Project.")
                    .ToActionResult();

            // TODO:
            return new OkResult();
            // var command = new ProjectUserUpdateCommand(CurrentUser, user, projectId);

            // return await orchestrator
            //     .InvokeAndReturnAccepted(command)
            //     .ConfigureAwait(false);
        });


        [HttpDelete("{tagKey}")]
        [Authorize(Policy = AuthPolicies.ProjectWrite)]
        [SwaggerOperation(OperationId = "DeleteProjectTag", Summary = "Deletes an existing Project Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the provided key was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Delete([FromRoute] string tagKey) => EnsureProjectAsync(project =>
        {
            if (string.IsNullOrWhiteSpace(tagKey))
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!project.Tags.TryGetValue(tagKey, out _))
                return ErrorResult
                    .NotFound($"The specified Tag could not be found in this Project.")
                    .ToActionResult();

            // TODO:
            return new NoContentResult();
            // var command = new ProjectUserDeleteCommand(CurrentUser, user, projectId);

            // return await orchestrator
            //     .InvokeAndReturnAccepted(command)
            //     .ConfigureAwait(false);
        });
    }
}

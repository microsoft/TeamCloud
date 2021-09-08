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
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;

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
        public Task<IActionResult> Get() => WithContextAsync<Project>((contextUser, project) =>
        {
            var tags = project?.Tags is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(project.Tags);

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
        public Task<IActionResult> Get([FromRoute] string tagKey) => WithContextAsync<Project>((contextUser, project) =>
        {
            if (string.IsNullOrWhiteSpace(tagKey))
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResultAsync();

            if (!project.Tags.TryGetValue(tagKey, out var tagValue))
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
        public Task<IActionResult> Post([FromBody] Dictionary<string, string> tags) => WithContextAsync<Project>(async (contextUser, project) =>
        {
            project.Tags = tags ?? new Dictionary<string, string>();

            var command = new ProjectUpdateCommand(contextUser, project);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpPut]
        [Authorize(Policy = AuthPolicies.ProjectAdmin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectTag", Summary = "Updates an existing Project Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the key provided in the request body was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Put([FromBody] Dictionary<string, string> tagsUpdate) => WithContextAsync<Project>(async (contextUser, project) =>
        {
            foreach (var kvp in tagsUpdate ?? new Dictionary<string, string>())
            {
                project.Tags ??= new Dictionary<string, string>();
                project.Tags[kvp.Key] = kvp.Value;
            }

            var command = new ProjectUpdateCommand(contextUser, project);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{tagKey}")]
        [Authorize(Policy = AuthPolicies.ProjectAdmin)]
        [SwaggerOperation(OperationId = "DeleteProjectTag", Summary = "Deletes an existing Project Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the provided key was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Delete([FromRoute] string tagKey) => WithContextAsync<Project>(async (contextUser, project) =>
        {
            if (string.IsNullOrWhiteSpace(tagKey))
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!project.Tags.Remove(tagKey))
                return ErrorResult
                    .NotFound($"The specified Tag could not be found in this Project.")
                    .ToActionResult();

            var command = new ProjectUpdateCommand(contextUser, project);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync(command, Request)
                .ConfigureAwait(false);
        });
    }
}

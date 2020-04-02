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
    [Route("api/projects/{projectId:guid}/tags")]
    [Produces("application/json")]
    public class ProjectTagsController : ControllerBase
    {
        readonly UserService userService;
        readonly Orchestrator orchestrator;
        readonly IProjectsRepositoryReadOnly projectsRepository;

        public ProjectTagsController(UserService userService, Orchestrator orchestrator, IProjectsRepositoryReadOnly projectsRepository)
        {
            this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
        }

        public Guid? ProjectId
        {
            get
            {
                var projectId = RouteData.Values.GetValueOrDefault(nameof(ProjectId), StringComparison.OrdinalIgnoreCase)?.ToString();

                return string.IsNullOrEmpty(projectId) ? null : (Guid?)Guid.Parse(projectId);
            }
        }

        private User CurrentUser => new User()
        {
            Id = userService.CurrentUserId,
            Role = UserRoles.Project.Owner
        };


        [HttpGet]
        [Authorize(Policy = "projectRead")]
        [SwaggerOperation(OperationId = "GetProjectTags", Summary = "Gets all Tags for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Tags", typeof(DataResult<Dictionary<string, string>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The projectId provided in the path was invalid.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{ProjectId.Value}' could not be found in this TeamCloud Instance.")
                    .ActionResult();

            var tags = project?.Tags is null ? new Dictionary<string, string>() : new Dictionary<string, string>(project.Tags);

            return DataResult<Dictionary<string, string>>
                .Ok(tags)
                .ActionResult();
        }


        [HttpGet("{tagKey}")]
        [Authorize(Policy = "projectRead")]
        [SwaggerOperation(OperationId = "GetProjectTagByKey", Summary = "Gets a Project Tag by Key.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project Tag", typeof(DataResult<Dictionary<string, string>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The projectId provided in the path was invalid.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the provided key was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get([FromRoute] string tagKey)
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ActionResult();

            if (string.IsNullOrWhiteSpace(tagKey))
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{ProjectId.Value}' could not be found in this TeamCloud Instance.")
                    .ActionResult();

            if (!project.Tags.TryGetValue(tagKey, out var tagValue))
                return ErrorResult
                    .NotFound($"The specified Tag could not be found in this Project.")
                    .ActionResult();

            return DataResult<Dictionary<string, string>>
                .Ok(new Dictionary<string, string> { { tagKey, tagValue } })
                .ActionResult();
        }


        [HttpPost]
        [Authorize(Policy = "projectCreate")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectTag", Summary = "Creates a new Project Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Project Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The projectId provided in the path was invalid, or the key provided in the request body was invalid.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Tag already exists with the key provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] Dictionary<string, string> tags)
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var tag = tags.FirstOrDefault();

            if (tag.Key is null)
                return ErrorResult
                    .BadRequest()
                    .ActionResult();

            var project = await projectsRepository
            .GetAsync(ProjectId.Value)
            .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{ProjectId.Value}' could not be found in this TeamCloud Instance.")
                    .ActionResult();

            if (project.Tags.ContainsKey(tag.Key))
                return ErrorResult
                    .Conflict($"A Tag with the key '{tag.Key}' already exists on this Project. Please try your request again with a unique key or call PUT to update the existing Tag.")
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
        [Authorize(Policy = "projectCreate")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectTag", Summary = "Updates an existing Project Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The projectId provided in the path was invalid, or the Tag provided in the request body did not pass validation.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the key provided in the request body was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromBody] Dictionary<string, string> tags)
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var tag = tags.FirstOrDefault();

            if (tag.Key is null)
                return ErrorResult
                    .BadRequest()
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{ProjectId.Value}' could not be found in this TeamCloud Instance.")
                    .ActionResult();


            if (!project.Tags.ContainsKey(tag.Key))
                return ErrorResult
                    .NotFound($"A Tag with the key '{tag.Key}' could not be found in this Project.")
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
        [Authorize(Policy = "projectCreate")]
        [SwaggerOperation(OperationId = "DeleteProjectTag", Summary = "Deletes an existing Project Tag.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project Tag. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "The projectId or key provided in the path was invalid.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the provided key was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete([FromRoute]string tagKey)
        {
            if (!ProjectId.HasValue)
                return ErrorResult
                    .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ActionResult();

            if (string.IsNullOrWhiteSpace(tagKey))
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId.Value)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the ID '{ProjectId.Value}' could not be found in this TeamCloud Instance.")
                    .ActionResult();
            if (!project.Tags.TryGetValue(tagKey, out _))
                return ErrorResult
                    .NotFound($"The specified Tag could not be found in this Project.")
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

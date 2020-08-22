/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Data;
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
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/links")]
    [Produces("application/json")]
    public class ProjectLinksController : ApiController
    {
        private readonly IProjectRepository projectRepository;
        private readonly IProjectLinkRepository projectLinkRepository;

        public ProjectLinksController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository, IProjectLinkRepository projectLinkRepository) : base(userService, orchestrator)
        {
            this.projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            this.projectLinkRepository = projectLinkRepository ?? throw new ArgumentNullException(nameof(projectLinkRepository));
        }

        private async Task<IActionResult> ProcessAsync(Func<Task<IActionResult>> callback)
        {
            try
            {
                if (callback is null)
                    throw new ArgumentNullException(nameof(callback));

                if (string.IsNullOrEmpty(ProjectId))
                {
                    return ErrorResult
                        .BadRequest($"Project Id provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                        .ToActionResult();
                }

                var project = await projectRepository
                    .GetAsync(ProjectId)
                    .ConfigureAwait(false);

                if (project is null)
                {
                    return ErrorResult
                        .NotFound($"A Project with the ID '{ProjectId}' could not be found in this TeamCloud Instance.")
                        .ToActionResult();
                }

                return await callback()
                    .ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                return ErrorResult
                    .ServerError(exc)
                    .ToActionResult();
            }
        }

        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectLinks", Summary = "Gets all Links for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Links", typeof(DataResult<Dictionary<string, string>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => ProcessAsync(async () =>
        {
            var linkDocuments = await projectLinkRepository
                .ListAsync(ProjectId)
                .ToListAsync()
                .ConfigureAwait(false);

            var links = linkDocuments
                .Select(linkDocument => linkDocument.PopulateExternalModel())
                .ToList();

            return DataResult<List<ProjectLink>>
                .Ok(links)
                .ToActionResult();
        });


        [HttpGet("{linkId}")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectLinkByKey", Summary = "Gets a Project Link by Key.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project Link", typeof(DataResult<Dictionary<string, string>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the provided key was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string linkId) => ProcessAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(linkId))
            {
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();
            }

            var linkDocument = await projectLinkRepository
                .GetAsync(ProjectId, linkId)
                .ConfigureAwait(false);

            if (linkDocument is null)
            {
                return ErrorResult
                    .NotFound($"A Link with the ID '{linkId}' could not be found for Project {ProjectId}.")
                    .ToActionResult();
            }

            var link = linkDocument
                .PopulateExternalModel();

            return DataResult<ProjectLink>
                .Ok(link)
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProjectWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectLink", Summary = "Creates a new Project Link.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The created Project Link.", typeof(DataResult<ProjectLink>))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Project Link. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Link already exists with the key provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] ProjectLinkDefinition linkDefinition) => ProcessAsync(async () =>
        {
            if (linkDefinition is null)
            {
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();
            }
            else if (!linkDefinition.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
            {
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();
            }

            var linkDocument = await projectLinkRepository
                .GetAsync(ProjectId, linkDefinition.Id)
                .ConfigureAwait(false);

            if (linkDocument != null)
            {
                return ErrorResult
                    .Conflict($"A Link with the ID '{linkDocument.Id}' already exists for Project {ProjectId}.")
                    .ToActionResult();
            }

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            linkDocument = new ProjectLinkDocument()
            {
                Id = linkDefinition.Id,
                ProjectId = ProjectId,
                HRef = linkDefinition.HRef,
                Title = linkDefinition.Title,
                Type = linkDefinition.Type
            };

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProjectLinkDocument, ProjectLink>(new OrchestratorProjectLinkCreateCommand(currentUser, linkDocument, ProjectId), Request)
                .ConfigureAwait(false);
        });


        [HttpPut("{linkId}")]
        [Authorize(Policy = AuthPolicies.ProjectWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectLink", Summary = "Updates an existing Project Link.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The Project Link was updated.", typeof(DataResult<ProjectLink>))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project Link. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the key provided in the request body was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Put([FromRoute] string linkId, [FromBody] ProjectLink link) => ProcessAsync(async () =>
        {
            if (!Guid.TryParse(linkId, out var linkIdParsed) || linkIdParsed == Guid.Empty)
            {
                return ErrorResult
                    .BadRequest($"The Link Id '{linkId}' provided in the url path is invalid. Must be a valid, non-empty GUID.", ResultErrorCode.ValidationError)
                    .ToActionResult();
            }

            if (link is null)
            {
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();
            }
            else if (!link.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
            {
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();
            }
            else if (!link.ProjectId.Equals(ProjectId, StringComparison.Ordinal))
            {
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"ProjectLink's ProjectId does match the identifier provided in the path." })
                    .ToActionResult();
            }
            else if (!link.Id.Equals(linkId, StringComparison.Ordinal))
            {
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"ProjectLink's Id does match the identifier provided in the path." })
                    .ToActionResult();
            }

            var linkDocument = await projectLinkRepository
                .GetAsync(ProjectId, link.Id)
                .ConfigureAwait(false);

            if (linkDocument is null)
            {
                return ErrorResult
                    .NotFound($"A Link with the ID '{link.Id}' could not be found for Project {ProjectId}.")
                    .ToActionResult();
            }

            linkDocument.PopulateFromExternalModel(link);

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProjectLinkDocument, ProjectLink>(new OrchestratorProjectLinkUpdateCommand(currentUser, linkDocument, ProjectId), Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{linkId}")]
        [Authorize(Policy = AuthPolicies.ProjectWrite)]
        [SwaggerOperation(OperationId = "DeleteProjectLink", Summary = "Deletes an existing Project Link.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project Link. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The Project Link was deleted.", typeof(DataResult<ProjectLink>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided Id was not found, or a Link with the provided Id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Delete([FromRoute] string linkId) => ProcessAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(linkId))
            {
                return ErrorResult
                    .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();
            }

            var linkDocument = await projectLinkRepository
                .GetAsync(ProjectId, linkId)
                .ConfigureAwait(false);

            if (linkDocument is null)
            {
                return ErrorResult
                    .NotFound($"A Link with the ID '{linkId}' could not be found in Project {ProjectId}.")
                    .ToActionResult();
            }

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProjectLinkDocument, ProjectLink>(new OrchestratorProjectLinkDeleteCommand(currentUser, linkDocument, ProjectId), Request)
                .ConfigureAwait(false);
        });
    }
}

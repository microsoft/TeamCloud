﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

// using System;
// using System.Collections.Generic;
// using System.Data;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Swashbuckle.AspNetCore.Annotations;
// using TeamCloud.API.Auth;
// using TeamCloud.API.Data.Results;
// using TeamCloud.API.Services;
// using TeamCloud.Data;
// using TeamCloud.Model.Commands;
// using TeamCloud.Model.Data;
// using TeamCloud.Validation;

// namespace TeamCloud.API.Controllers
// {
//     [ApiController]
//     [Route("api/projects/{projectId:projectId}/links")]
//     [Produces("application/json")]
//     public class ProjectLinksController : ApiController
//     {
//         private readonly IProjectLinkRepository projectLinkRepository;

//         public ProjectLinksController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository, IProjectLinkRepository projectLinkRepository)
//             : base(userService, orchestrator, projectRepository)
//         {
//             this.projectLinkRepository = projectLinkRepository ?? throw new ArgumentNullException(nameof(projectLinkRepository));
//         }


//         [HttpGet]
//         [Authorize(Policy = AuthPolicies.ProjectRead)]
//         [SwaggerOperation(OperationId = "GetProjectLinks", Summary = "Gets all Links for a Project.")]
//         [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Links", typeof(DataResult<List<ProjectLink>>))]
//         [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
//         [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
//         public Task<IActionResult> Get() => EnsureProjectIdAsync(async project =>
//         {
//             var linkDocuments = await projectLinkRepository
//                 .ListAsync(project.Id)
//                 .ToListAsync()
//                 .ConfigureAwait(false);

//             var links = linkDocuments
//                 .Select(linkDocument => linkDocument.PopulateExternalModel())
//                 .ToList();

//             return DataResult<List<ProjectLink>>
//                 .Ok(links)
//                 .ToActionResult();
//         });


//         [HttpGet("{linkId}")]
//         [Authorize(Policy = AuthPolicies.ProjectRead)]
//         [SwaggerOperation(OperationId = "GetProjectLinkByKey", Summary = "Gets a Project Link by Key.")]
//         [SwaggerResponse(StatusCodes.Status200OK, "Returns Project Link", typeof(DataResult<ProjectLink>))]
//         [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
//         [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the provided key was not found.", typeof(ErrorResult))]
//         public Task<IActionResult> Get([FromRoute] string linkId) => EnsureProjectIdAsync(async project =>
//         {
//             if (string.IsNullOrWhiteSpace(linkId))
//                 return ErrorResult
//                     .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
//                     .ToActionResult();

//             var linkDocument = await projectLinkRepository
//                 .GetAsync(project.Id, linkId)
//                 .ConfigureAwait(false);

//             if (linkDocument is null)
//                 return ErrorResult
//                     .NotFound($"A Link with the ID '{linkId}' could not be found for Project {project.Id}.")
//                     .ToActionResult();

//             var link = linkDocument
//                 .PopulateExternalModel();

//             return DataResult<ProjectLink>
//                 .Ok(link)
//                 .ToActionResult();
//         });


//         [HttpPost]
//         [Authorize(Policy = AuthPolicies.ProjectLinkWrite)]
//         [Consumes("application/json")]
//         [SwaggerOperation(OperationId = "CreateProjectLink", Summary = "Creates a new Project Link.")]
//         [SwaggerResponse(StatusCodes.Status201Created, "The created Project Link.", typeof(DataResult<ProjectLink>))]
//         [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Project Link. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
//         [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
//         [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
//         [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Link already exists with the key provided in the request body.", typeof(ErrorResult))]
//         public Task<IActionResult> Post([FromBody] ProjectLink link) => EnsureProjectIdAsync(async project =>
//         {
//             if (link is null)
//                 return ErrorResult
//                     .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
//                     .ToActionResult();

//             if (!link.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
//                 return ErrorResult
//                     .BadRequest(validationResult)
//                     .ToActionResult();

//             var linkDocument = await projectLinkRepository
//                 .GetAsync(project.Id, link.Id)
//                 .ConfigureAwait(false);

//             if (linkDocument is not null)
//                 return ErrorResult
//                     .Conflict($"A Link with the ID '{linkDocument.Id}' already exists for Project {project.Id}.")
//                     .ToActionResult();

//             var currentUser = await UserService
//                 .CurrentUserAsync()
//                 .ConfigureAwait(false);

//             linkDocument = new ProjectLink
//             {
//                 Id = link.Id,
//                 ProjectId = project.Id,
//                 HRef = link.HRef,
//                 Title = link.Title,
//                 Type = link.Type
//             };

//             return await Orchestrator
//                 .InvokeAndReturnActionResultAsync<ProjectLink, ProjectLink>(new ProjectLinkCreateCommand(currentUser, linkDocument, project.Id), Request)
//                 .ConfigureAwait(false);
//         });


//         [HttpPut("{linkId}")]
//         [Authorize(Policy = AuthPolicies.ProjectLinkWrite)]
//         [Consumes("application/json")]
//         [SwaggerOperation(OperationId = "UpdateProjectLink", Summary = "Updates an existing Project Link.")]
//         [SwaggerResponse(StatusCodes.Status200OK, "The Project Link was updated.", typeof(DataResult<ProjectLink>))]
//         [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project Link. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
//         [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
//         [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Tag with the key provided in the request body was not found.", typeof(ErrorResult))]
//         public Task<IActionResult> Put([FromRoute] string linkId, [FromBody] ProjectLink link) => EnsureProjectIdAsync(async project =>
//         {
//             if (!Guid.TryParse(linkId, out var linkIdParsed) || linkIdParsed == Guid.Empty)
//                 return ErrorResult
//                     .BadRequest($"The Link Id '{linkId}' provided in the url path is invalid. Must be a valid, non-empty GUID.", ResultErrorCode.ValidationError)
//                     .ToActionResult();

//             if (link is null)
//                 return ErrorResult
//                     .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
//                     .ToActionResult();

//             if (!link.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
//                 return ErrorResult
//                     .BadRequest(validationResult)
//                     .ToActionResult();

//             if (!link.Id.Equals(linkId, StringComparison.Ordinal))
//                 return ErrorResult
//                     .BadRequest(new ValidationError { Field = "id", Message = $"ProjectLink's Id does match the identifier provided in the path." })
//                     .ToActionResult();

//             var linkDocument = await projectLinkRepository
//                 .GetAsync(project.Id, link.Id)
//                 .ConfigureAwait(false);

//             if (linkDocument is null)
//                 return ErrorResult
//                     .NotFound($"A Link with the ID '{link.Id}' could not be found for Project {project.Id}.")
//                     .ToActionResult();

//             linkDocument.PopulateFromExternalModel(link);

//             var currentUser = await UserService
//                 .CurrentUserAsync()
//                 .ConfigureAwait(false);

//             return await Orchestrator
//                 .InvokeAndReturnActionResultAsync<ProjectLink, ProjectLink>(new ProjectLinkUpdateCommand(currentUser, linkDocument, project.Id), Request)
//                 .ConfigureAwait(false);
//         });


//         [HttpDelete("{linkId}")]
//         [Authorize(Policy = AuthPolicies.ProjectLinkWrite)]
//         [SwaggerOperation(OperationId = "DeleteProjectLink", Summary = "Deletes an existing Project Link.")]
//         [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project Link. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
//         [SwaggerResponse(StatusCodes.Status204NoContent, "The Project Link was deleted.", typeof(DataResult<ProjectLink>))]
//         [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
//         [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided Id was not found, or a Link with the provided Id was not found.", typeof(ErrorResult))]
//         public Task<IActionResult> Delete([FromRoute] string linkId) => EnsureProjectIdAsync(async project =>
//         {
//             if (string.IsNullOrWhiteSpace(linkId))
//                 return ErrorResult
//                     .BadRequest($"The key provided in the url path is invalid.  Must be a non-empty string.", ResultErrorCode.ValidationError)
//                     .ToActionResult();

//             var linkDocument = await projectLinkRepository
//                 .GetAsync(project.Id, linkId)
//                 .ConfigureAwait(false);

//             if (linkDocument is null)
//                 return ErrorResult
//                     .NotFound($"A Link with the ID '{linkId}' could not be found in Project {project.Id}.")
//                     .ToActionResult();

//             var currentUser = await UserService
//                 .CurrentUserAsync()
//                 .ConfigureAwait(false);

//             return await Orchestrator
//                 .InvokeAndReturnActionResultAsync<ProjectLink, ProjectLink>(new ProjectLinkDeleteCommand(currentUser, linkDocument, project.Id), Request)
//                 .ConfigureAwait(false);
//         });
//     }
// }

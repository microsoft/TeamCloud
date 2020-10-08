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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Auth;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using ValidationError = TeamCloud.API.Data.Results.ValidationError;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/components")]
    [Produces("application/json")]
    public class ProjectComponentsController : ApiController
    {
        private readonly IComponentRepository componentRepository;
        private readonly IComponentOfferRepository componentOfferRepository;

        public ProjectComponentsController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository, IComponentRepository componentRepository, IComponentOfferRepository componentOfferRepository)
            : base(userService, orchestrator, projectRepository)
        {
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
            this.componentOfferRepository = componentOfferRepository ?? throw new ArgumentNullException(nameof(componentOfferRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectComponents", Summary = "Gets all Components for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Project Components", typeof(DataResult<List<Component>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get() => EnsureProjectAsync(async project =>
        {
            var componentDocuments = await componentRepository
                .ListAsync(project.Id)
                .ToListAsync()
                .ConfigureAwait(false);

            var components = componentDocuments
                .Select(componentDocument => componentDocument.PopulateExternalModel())
                .ToList();

            return DataResult<List<Component>>
                .Ok(components)
                .ToActionResult();
        });


        [HttpGet("{componentId}")]
        [Authorize(Policy = AuthPolicies.ProjectRead)]
        [SwaggerOperation(OperationId = "GetProjectComponentById", Summary = "Gets a Project Component by id.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns Project Component", typeof(DataResult<Component>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Component with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string componentId) => EnsureProjectAsync(async project =>
        {
            if (string.IsNullOrWhiteSpace(componentId))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var componentDocument = await componentRepository
                .GetAsync(project.Id, componentId)
                .ConfigureAwait(false);

            if (componentDocument is null)
                return ErrorResult
                    .NotFound($"A Component with the ID '{componentId}' could not be found for Project {project.Id}.")
                    .ToActionResult();

            var component = componentDocument.PopulateExternalModel();

            return DataResult<Component>
                .Ok(component)
                .ToActionResult();
        });


        [HttpPost]
        // TODO: update auth policy
        [Authorize(Policy = AuthPolicies.ProjectLinkWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectComponent", Summary = "Creates a new Project Component.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The created Project Component.", typeof(DataResult<Component>))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Project Component. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Component already exists with the id provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] ComponentRequest request) => EnsureProjectAsync(async project =>
        {
            if (request is null)
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            // if (!component.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
            //     return ErrorResult
            //         .BadRequest(validationResult)
            //         .ToActionResult();

            var offerDocument = await componentOfferRepository
                .GetAsync(request.OfferId)
                .ConfigureAwait(false);

            if (offerDocument is null)
                return ErrorResult
                    .NotFound($"A ComponentOffer with the id '{request.OfferId}' could not be found for Project {project.Id}.")
                    .ToActionResult();

            if (!project.Type.Providers.Any(p => p.Id.Equals(offerDocument.ProviderId, StringComparison.Ordinal)))
                return ErrorResult
                    .NotFound($"A ComponentOffer with the id '{request.OfferId}' could not be found for Project {project.Id}.")
                    .ToActionResult();

            var input = JObject.Parse(request.InputJson);
            var schema = JSchema.Parse(offerDocument.InputJsonSchema);

            if (!input.IsValid(schema, out IList<string> schemaErrors))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "input", Message = $"ComponentRequest's input does not match the the Offer inputJsonSchema.  Errors: {string.Join(", ", schemaErrors)}." })
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var componentDocument = new ComponentDocument
            {
                OfferId = offerDocument.Id,
                ProjectId = project.Id,
                ProviderId = offerDocument.ProviderId,
                RequestedBy = currentUser.Id,
                InputJson = request.InputJson
            };

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ComponentDocument, Component>(new OrchestratorProjectComponentCreateCommand(currentUser, componentDocument, project.Id), Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{componentId}")]
        [Authorize(Policy = AuthPolicies.ProjectLinkWrite)]
        [SwaggerOperation(OperationId = "DeleteProjectComponent", Summary = "Deletes an existing Project Component.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the Project Component. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The Project Component was deleted.", typeof(DataResult<Component>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided id was not found, or a Component with the provided id was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Delete([FromRoute] string componentId) => EnsureProjectAsync(async project =>
        {
            if (string.IsNullOrWhiteSpace(componentId))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var componentDocument = await componentRepository
                .GetAsync(project.Id, componentId)
                .ConfigureAwait(false);

            if (componentDocument is null)
                return ErrorResult
                    .NotFound($"A Component with the id '{componentDocument.Id}' could not be found for Project {project.Id}.")
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ComponentDocument, Component>(new OrchestratorProjectComponentDeleteCommand(currentUser, componentDocument, project.Id), Request)
                .ConfigureAwait(false);
        });
    }
}

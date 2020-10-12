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
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Data;
using TeamCloud.Model.Validation;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/providers/{providerId:providerId}/components")]
    [Produces("application/json")]
    public class ProviderProjectComponentsController : ApiController
    {
        private readonly IComponentRepository componentRepository;

        public ProviderProjectComponentsController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository, IComponentRepository componentRepository)
            : base(userService, orchestrator, projectRepository)
        {
            this.componentRepository = componentRepository ?? throw new ArgumentNullException(nameof(componentRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProviderComponentWrite)]
        [SwaggerOperation(OperationId = "GetProviderProjectComponents", Summary = "Gets all Components for a Project.")]
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
        [Authorize(Policy = AuthPolicies.ProviderComponentWrite)]
        [SwaggerOperation(OperationId = "GetProviderProjectComponentById", Summary = "Gets a Project Component by id.")]
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
        [Authorize(Policy = AuthPolicies.ProviderComponentWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProviderProjectComponent", Summary = "Creates a new Project Component.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The created Project Component.", typeof(DataResult<Component>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project Component already exists with the id provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] Component component) => EnsureProjectAsync(async project =>
        {
            if (component is null)
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!component.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            var componentDocument = await componentRepository
                .GetAsync(project.Id, component.Id)
                .ConfigureAwait(false);

            if (componentDocument != null)
                return ErrorResult
                    .Conflict($"A Component with the ID '{componentDocument.Id}' already exists for Project {project.Id}.")
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            // TODO: validate offerId

            component.ProjectId = project.Id;
            component.ProviderId = ProviderId;

            componentDocument = new ComponentDocument().PopulateFromExternalModel(component);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ComponentDocument, Component>(new OrchestratorComponentCreateCommand(currentUser, componentDocument, project.Id), Request)
                .ConfigureAwait(false);
        });


        [HttpPut("{componentId}")]
        [Authorize(Policy = AuthPolicies.ProviderComponentWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProviderProjectComponent", Summary = "Updates an existing Project Component.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The Project Component was updated.", typeof(DataResult<Component>))]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the Project Component. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a Component with the id provided in the request body was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Put([FromRoute] string componentId, [FromBody] Component component) => EnsureProjectAsync(async project =>
        {
            if (string.IsNullOrWhiteSpace(componentId))
                return ErrorResult
                    .BadRequest($"The id provided in the url path is invalid. Must be a non-empty string.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (component is null)
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!component.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            if (!component.Id.Equals(componentId, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"Component's id does not match the identifier provided in the path." })
                    .ToActionResult();

            var componentDocument = await componentRepository
                .GetAsync(project.Id, component.Id)
                .ConfigureAwait(false);

            if (componentDocument is null)
                return ErrorResult
                    .NotFound($"A Component with the id '{component.Id}' could not be found for Project {project.Id}.")
                    .ToActionResult();

            componentDocument.PopulateFromExternalModel(component);

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ComponentDocument, Component>(new OrchestratorComponentUpdateCommand(currentUser, componentDocument, project.Id), Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{componentId}")]
        [Authorize(Policy = AuthPolicies.ProviderComponentWrite)]
        [SwaggerOperation(OperationId = "DeleteProviderProjectComponent", Summary = "Deletes an existing Project Component.")]
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
                .InvokeAndReturnActionResultAsync<ComponentDocument, Component>(new OrchestratorComponentDeleteCommand(currentUser, componentDocument, project.Id), Request)
                .ConfigureAwait(false);
        });
    }
}

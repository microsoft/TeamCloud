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
    [Route("api/projects/{projectId:guid}/providers/{providerId:providerId}/data")]
    [Produces("application/json")]
    public class ProjectProviderDataController : ApiController
    {
        private readonly IProviderDataRepository providerDataRepository;

        public ProjectProviderDataController(UserService userService, Orchestrator orchestrator, IProjectRepository projectRepository, IProviderRepository providerRepository, IProviderDataRepository providerDataRepository)
            : base(userService, orchestrator, projectRepository, providerRepository)
        {
            this.providerDataRepository = providerDataRepository ?? throw new ArgumentNullException(nameof(providerDataRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.ProviderDataRead)]
        [SwaggerOperation(OperationId = "GetProjectProviderData", Summary = "Gets the ProviderData items for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns ProviderData", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the provided id was not found, or a Project with the provided identifier was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromQuery] bool includeShared) => EnsureProjectAndProviderAsync(async (project, provider) =>
        {
            if (!project.Type.Providers.Any(p => p.Id.Equals(provider.Id, StringComparison.OrdinalIgnoreCase)))
                return ErrorResult
                    .NotFound($"A Provider with the ID '{provider.Id}' could not be found on the Project '{project.Id}'")
                    .ToActionResult();

            var dataDocuments = await providerDataRepository
                .ListAsync(provider.Id, project.Id, includeShared)
                .ToListAsync()
                .ConfigureAwait(false);

            var data = dataDocuments.Select(d => d.PopulateExternalModel()).ToList();

            return DataResult<List<ProviderData>>
                .Ok(data)
                .ToActionResult();
        });


        [HttpGet("{providerDataId:guid}")]
        [Authorize(Policy = AuthPolicies.ProviderDataRead)]
        [SwaggerOperation(OperationId = "GetProjectProviderDataById", Summary = "Gets a ProviderData for a Project by ID.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns ProviderData", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ProviderData with the provided id was not found, or a Project with the provided identifier was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Get([FromRoute] string providerDataId) => EnsureProjectAndProviderAsync(async (project, provider) =>
        {
            if (!project.Type.Providers.Any(p => p.Id.Equals(provider.Id, StringComparison.OrdinalIgnoreCase)))
                return ErrorResult
                    .NotFound($"A Provider with the ID '{provider.Id}' could not be found on the Project '{project.Id}'")
                    .ToActionResult();

            var dataDocument = await providerDataRepository
                .GetAsync(providerDataId)
                .ConfigureAwait(false);

            if (dataDocument is null)
                return ErrorResult
                    .NotFound($"A Provider Data item with the ID '{providerDataId}' could not be found")
                    .ToActionResult();

            var data = dataDocument.PopulateExternalModel();

            return DataResult<ProviderData>
                .Ok(data)
                .ToActionResult();
        });


        [HttpPost]
        [Authorize(Policy = AuthPolicies.ProviderDataWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectProviderData", Summary = "Creates a new ProviderData")]
        [SwaggerResponse(StatusCodes.Status201Created, "The ProviderData was created.", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the provided provider ID was not found, or a Project with the id specified was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public Task<IActionResult> Post([FromBody] ProviderData providerData) => EnsureProjectAndProviderAsync(async (project, provider) =>
        {
            if (providerData is null)
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!providerData.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            if (!project.Type.Providers.Any(p => p.Id.Equals(provider.Id, StringComparison.OrdinalIgnoreCase)))
                return ErrorResult
                    .NotFound($"A Provider with the ID '{provider.Id}' could not be found on the Project '{project.Id}'")
                    .ToActionResult();

            var dataDocument = new ProviderDataDocument
            {
                ProviderId = provider.Id,
                Scope = ProviderDataScope.Project,
                ProjectId = project.Id,

            }.PopulateFromExternalModel(providerData);

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProviderDataDocument, ProviderData>(new OrchestratorProviderDataCreateCommand(currentUser, dataDocument), Request)
                .ConfigureAwait(false);
        });


        [HttpPut("{providerDataId:guid}")]
        [Authorize(Policy = AuthPolicies.ProviderDataWrite)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectProviderData", Summary = "Updates an existing ProviderData.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The ProviderData was updated.", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Put([FromRoute] string providerDataId, [FromBody] ProviderData providerData) => EnsureProjectAndProviderAsync(async (project, provider) =>
        {
            if (providerData is null)
                return ErrorResult
                    .BadRequest($"The request body must not be EMPTY.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!providerData.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            if (string.IsNullOrWhiteSpace(providerDataId))
                return ErrorResult
                    .BadRequest($"The identifier '{providerDataId}' provided in the url path is invalid.  Must be a valid GUID.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!providerDataId.Equals(providerData.Id, StringComparison.OrdinalIgnoreCase))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"ProviderData's id does match the identifier provided in the path." })
                    .ToActionResult();

            if (!project.Type.Providers.Any(p => p.Id.Equals(provider.Id, StringComparison.OrdinalIgnoreCase)))
                return ErrorResult
                    .NotFound($"A Provider with the ID '{provider.Id}' could not be found on the Project '{project.Id}'")
                    .ToActionResult();

            var dataDocument = await providerDataRepository
                .GetAsync(providerData.Id)
                .ConfigureAwait(false);

            if (dataDocument is null)
                return ErrorResult
                    .NotFound($"The Provider Data '{providerData.Id}' could not be found..")
                    .ToActionResult();

            dataDocument = new ProviderDataDocument
            {
                ProviderId = provider.Id,
                Scope = ProviderDataScope.Project,
                ProjectId = project.Id,

            }.PopulateFromExternalModel(providerData);

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProviderDataDocument, ProviderData>(new OrchestratorProviderDataUpdateCommand(currentUser, dataDocument), Request)
                .ConfigureAwait(false);
        });


        [HttpDelete("{providerDataId:guid}")]
        [Authorize(Policy = AuthPolicies.ProviderDataWrite)]
        [SwaggerOperation(OperationId = "DeleteProjectProviderData", Summary = "Deletes a ProviderData.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The ProviderData was deleted.", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ProviderData with the providerDataId provided was not found.", typeof(ErrorResult))]
        public Task<IActionResult> Delete([FromRoute] string providerDataId) => EnsureProjectAndProviderAsync(async (project, provider) =>
        {
            var dataDocument = await providerDataRepository
                .GetAsync(providerDataId)
                .ConfigureAwait(false);

            if (dataDocument is null)
                return ErrorResult
                    .NotFound($"A Provider Data item with the ID '{providerDataId}' could not be found")
                    .ToActionResult();

            if (dataDocument.Scope == ProviderDataScope.System)
                return ErrorResult
                    .BadRequest("The specified Provider Data item is not scoped to a project use the system api to delete.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!dataDocument.ProjectId.Equals(project.Id, StringComparison.OrdinalIgnoreCase))
                return ErrorResult
                    .NotFound($"A Provider Data item with the ID '{providerDataId}' could not be found for project '{project.Id}'")
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProviderDataDocument, ProviderData>(new OrchestratorProviderDataDeleteCommand(currentUser, dataDocument), Request)
                .ConfigureAwait(false);
        });
    }
}

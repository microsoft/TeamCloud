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
    [Route("api/projectTypes")]
    [Produces("application/json")]
    public class ProjectTypesController : ApiController
    {
        private readonly IProjectTypeRepository projectTypeRepository;

        public ProjectTypesController(UserService userService, Orchestrator orchestrator, IProviderRepository providerRepository, IProjectTypeRepository projectTypeRepository)
            : base(userService, orchestrator, providerRepository)
        {
            this.projectTypeRepository = projectTypeRepository ?? throw new ArgumentNullException(nameof(projectTypeRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "GetProjectTypes", Summary = "Gets all Project Types.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all ProjectTypes.", typeof(DataResult<List<ProjectType>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var projectTypes = await projectTypeRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            var returnProjectTypes = projectTypes
                .Select(p => p.PopulateExternalModel())
                .ToList();

            return DataResult<List<ProjectType>>
                .Ok(returnProjectTypes)
                .ToActionResult();
        }


        [HttpGet("{projectTypeId}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "GetProjectTypeById", Summary = "Gets a Project Type by ID.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a ProjectType.", typeof(DataResult<ProjectType>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ProjectType with the projectTypeId provided was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get(string projectTypeId)
        {
            var projectType = await projectTypeRepository
                .GetAsync(projectTypeId)
                .ConfigureAwait(false);

            if (projectType is null)
                return ErrorResult
                    .NotFound($"A ProjectType with the ID '{projectTypeId}' could not be found in this TeamCloud Instance")
                    .ToActionResult();

            var returnProjectType = projectType.PopulateExternalModel();

            return DataResult<ProjectType>
                .Ok(returnProjectType)
                .ToActionResult();
        }


        [HttpPost]
        [Authorize(Policy = AuthPolicies.Admin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectType", Summary = "Creates a new Project Type.")]
        [SwaggerResponse(StatusCodes.Status201Created, "The new ProjectType was created.", typeof(DataResult<ProjectType>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A ProjectType already exists with the ID provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] ProjectType projectType)
        {
            if (projectType is null)
                return ErrorResult
                    .BadRequest("Request body must not be empty.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!projectType.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            var existingProjectType = await projectTypeRepository
                .GetAsync(projectType.Id)
                .ConfigureAwait(false);

            if (existingProjectType != null)
                return ErrorResult
                    .Conflict($"A ProjectType with id '{projectType.Id}' already exists.  Please try your request again with a unique id or call PUT to update the existing ProjectType.")
                    .ToActionResult();

            var providers = await ProviderRepository
                .ListAsync(includeServiceProviders: false)
                .ToListAsync()
                .ConfigureAwait(false);

            var validProviders = projectType.Providers
                .All(p => providers.Any(provider => provider.Id == p.Id));

            if (!validProviders)
            {
                var validProviderIds = string.Join(", ", providers.Select(p => p.Id));

                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectType", Message = $"All provider ids on a ProjectType must match the id of a registered Provider on the TeamCloud instance and cannot be a Service Provider. Valid provider ids are: {validProviderIds}" })
                    .ToActionResult();
            }

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var projectTypeDocument = new ProjectTypeDocument()
                .PopulateFromExternalModel(projectType);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProjectTypeDocument, ProjectType>(new OrchestratorProjectTypeCreateCommand(currentUser, projectTypeDocument), Request)
                .ConfigureAwait(false);
        }


        [HttpPut("{projectTypeId}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectType", Summary = "Updates an existing Project Type.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The ProjectType was updated.", typeof(DataResult<ProjectType>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project Type with the ID provided in the request body could not be found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromRoute] string projectTypeId, [FromBody] ProjectType projectType)
        {
            if (string.IsNullOrWhiteSpace(projectTypeId))
                return ErrorResult
                    .BadRequest($"The identifier '{projectTypeId}' provided in the url path is invalid.  Must be a valid project type ID.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (projectType is null)
                return ErrorResult
                    .BadRequest("Request body must not be empty.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            if (!projectType.TryValidate(out var validationResult, serviceProvider: HttpContext.RequestServices))
                return ErrorResult
                    .BadRequest(validationResult)
                    .ToActionResult();

            if (!projectType.Id.Equals(projectTypeId, StringComparison.Ordinal))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"ProjectType's id does match the identifier provided in the path." })
                    .ToActionResult();

            var projectTypeDocument = await projectTypeRepository
                .GetAsync(projectType.Id)
                .ConfigureAwait(false);

            if (projectTypeDocument is null)
                return ErrorResult
                    .NotFound($"A ProjectType with the ID '{projectType.Id}' could not be found in this TeamCloud Instance")
                    .ToActionResult();

            var providers = await ProviderRepository
                .ListAsync(includeServiceProviders: false)
                .ToListAsync()
                .ConfigureAwait(false);

            var validProviders = projectType.Providers
                .All(p => providers.Any(provider => provider.Id == p.Id));

            if (!validProviders)
            {
                var validProviderIds = string.Join(", ", providers.Select(p => p.Id));

                return ErrorResult
                    .BadRequest(new ValidationError { Field = "projectType", Message = $"All provider ids on a ProjectType must match the id of a registered Provider on the TeamCloud instance and cannot be a Service Provider. Valid provider ids are: {validProviderIds}" })
                    .ToActionResult();
            }

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            projectTypeDocument
                .PopulateFromExternalModel(projectType);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProjectTypeDocument, ProjectType>(new OrchestratorProjectTypeUpdateCommand(currentUser, projectTypeDocument), Request)
                .ConfigureAwait(false);
        }


        [HttpDelete("{projectTypeId}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "DeleteProjectType", Summary = "Deletes a Project Type.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The ProjectType was deleted.", typeof(DataResult<ProjectType>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ProjectType with the projectTypeId provided was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete([FromRoute] string projectTypeId)
        {
            var projectTypeDocument = await projectTypeRepository
                .GetAsync(projectTypeId)
                .ConfigureAwait(false);

            if (projectTypeDocument is null)
                return ErrorResult
                    .NotFound($"A ProjectType with the ID '{projectTypeId}' could not be found in this TeamCloud Instance")
                    .ToActionResult();

            var currentUser = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProjectTypeDocument, ProjectType>(new OrchestratorProjectTypeDeleteCommand(currentUser, projectTypeDocument), Request)
                .ConfigureAwait(false);
        }
    }
}

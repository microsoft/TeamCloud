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
using TeamCloud.Model.Validation.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/providers")]
    [Produces("application/json")]
    public class ProvidersController : ApiController
    {
        private readonly IProjectRepository projectsRepository;
        private readonly IProviderRepository providersRepository;
        private readonly IProjectTypeRepository projectTypesRepository;

        public ProvidersController(UserService userService, Orchestrator orchestrator, IProjectRepository projectsRepository, IProviderRepository providersRepository, IProjectTypeRepository projectTypesRepository) : base(userService, orchestrator)
        {
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
            this.projectTypesRepository = projectTypesRepository ?? throw new ArgumentNullException(nameof(projectTypesRepository));
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "GetProviders", Summary = "Gets all Providers.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns all Providers.", typeof(DataResult<List<Provider>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        public async Task<IActionResult> Get()
        {
            var providers = await providersRepository
                .ListAsync()
                .ToListAsync()
                .ConfigureAwait(false);

            var returnProviders = providers.Select(p => p.PopulateExternalModel()).ToList();

            return DataResult<List<Provider>>
                .Ok(returnProviders)
                .ToActionResult();
        }


        [HttpGet("{providerId}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "GetProviderById", Summary = "Gets a Provider by ID.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns a DataResult with the Provider as the data value.", typeof(DataResult<Provider>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the providerId provided was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get(string providerId)
        {
            var provider = await providersRepository
                .GetAsync(providerId)
                .ConfigureAwait(false);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{providerId}' could not be found in this TeamCloud Instance")
                    .ToActionResult();

            var returnProvider = provider.PopulateExternalModel();

            return DataResult<Provider>
                .Ok(returnProvider)
                .ToActionResult();
        }


        [HttpPost]
        [Authorize(Policy = AuthPolicies.Admin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProvider", Summary = "Creates a new Provider.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts creating the new Provider. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Provider already exists with the ID provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] Provider provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            var validation = new ProviderValidator().Validate(provider);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            var existingProvider = await providersRepository
                .GetAsync(provider.Id)
                .ConfigureAwait(false);

            if (existingProvider != null)
                return ErrorResult
                    .Conflict($"A Provider with the ID '{provider.Id}' already exists on this TeamCloud Instance. Please try your request again with a unique ID or call PUT to update the existing Provider.")
                    .ToActionResult();

            var currentUserForCommand = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var commandProvider = new ProviderDocument()
                .PopulateFromExternalModel(provider);

            var command = new OrchestratorProviderCreateCommand(currentUserForCommand, commandProvider);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProviderDocument, Provider>(command, Request)
                .ConfigureAwait(false);
        }


        [HttpPut("{providerId}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProvider", Summary = "Updates an existing Provider.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts updating the provided Provider. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the ID provided in the reques body was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromRoute] string providerId, [FromBody] Provider provider)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            if (string.IsNullOrWhiteSpace(providerId))
                return ErrorResult
                    .BadRequest($"The identifier '{providerId}' provided in the url path is invalid.  Must be a valid provider ID.", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var validation = new ProviderValidator().Validate(provider);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ToActionResult();

            if (!providerId.Equals(provider.Id, StringComparison.OrdinalIgnoreCase))
                return ErrorResult
                    .BadRequest(new ValidationError { Field = "id", Message = $"Provider's id does match the identifier provided in the path." })
                    .ToActionResult();

            var oldProvider = await providersRepository
                .GetAsync(provider.Id)
                .ConfigureAwait(false);

            if (oldProvider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{provider.Id}' could not be found on this TeamCloud Instance.")
                    .ToActionResult();

            var currentUserForCommand = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            oldProvider.PopulateFromExternalModel(provider);

            var command = new OrchestratorProviderUpdateCommand(currentUserForCommand, oldProvider);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProviderDocument, Provider>(command, Request)
                .ConfigureAwait(false);
        }


        [HttpDelete("{providerId}")]
        [Authorize(Policy = AuthPolicies.Admin)]
        [SwaggerOperation(OperationId = "DeleteProvider", Summary = "Deletes an existing Provider.")]
        [SwaggerResponse(StatusCodes.Status202Accepted, "Starts deleting the provided Provider. Returns a StatusResult object that can be used to track progress of the long-running operation.", typeof(StatusResult))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the provided providerId was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete(string providerId)
        {
            var provider = await providersRepository
                .GetAsync(providerId)
                .ConfigureAwait(false);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{providerId}' could not be found on this TeamCloud Instance.")
                    .ToActionResult();

            var projectTypes = await projectTypesRepository
                .ListByProviderAsync(providerId)
                .ToListAsync()
                .ConfigureAwait(false);

            if (projectTypes.Any())
                return ErrorResult
                    .BadRequest("Cannot delete Providers referenced in existing ProjectType definitions", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var projects = await projectsRepository
                .ListByProviderAsync(providerId)
                .ToListAsync()
                .ConfigureAwait(false);

            if (projects.Any())
                return ErrorResult
                    .BadRequest("Cannot delete Providers being used by existing Projects", ResultErrorCode.ValidationError)
                    .ToActionResult();

            var currentUserForCommand = await UserService
                .CurrentUserAsync()
                .ConfigureAwait(false);

            var command = new OrchestratorProviderDeleteCommand(currentUserForCommand, provider);

            return await Orchestrator
                .InvokeAndReturnActionResultAsync<ProviderDocument, Provider>(command, Request)
                .ConfigureAwait(false);
        }
    }
}
